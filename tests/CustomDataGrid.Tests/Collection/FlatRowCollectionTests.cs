using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using CustomDataGrid.Collection;
using CustomDataGrid.Contracts;
using CustomDataGrid.Contracts.Events;
using CustomDataGrid.Models;
using Moq;
using NUnit.Framework;

namespace CustomDataGrid.Tests.Collection
{
    [TestFixture]
    public class FlatRowCollectionTests
    {
        // ------------------------------------------------------------------ //
        //  Helpers                                                            //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Builds a mock IGridDataSource with <paramref name="groupCount"/> groups,
        /// each containing <paramref name="itemsPerGroup"/> items (all enabled).
        /// </summary>
        private static Mock<IGridDataSource> MakeSource(int groupCount, int itemsPerGroup = 0)
        {
            var mock = new Mock<IGridDataSource>();
            mock.Setup(s => s.GroupCount).Returns(groupCount);

            for (int g = 0; g < groupCount; g++)
            {
                int gi = g; // capture
                var group = new GridGroupRow { Label = $"G{gi}", IsEnabled = true };
                mock.Setup(s => s.GetGroup(gi)).Returns(group);
                mock.Setup(s => s.GetItemCount(gi)).Returns(itemsPerGroup);

                for (int i = 0; i < itemsPerGroup; i++)
                {
                    int ii = i;
                    var item = new GridItemRow();
                    mock.Setup(s => s.GetItem(gi, ii)).Returns(item);
                }
            }

            mock.SetupAdd(s => s.GroupChanged += It.IsAny<EventHandler<GroupChangedEventArgs>>());
            mock.SetupAdd(s => s.ItemChanged += It.IsAny<EventHandler<ItemChangedEventArgs>>());
            mock.SetupAdd(s => s.DataReset += It.IsAny<EventHandler>());
            return mock;
        }

        /// <summary>
        /// Collects all CollectionChanged events fired on <paramref name="frc"/>
        /// into a list for assertion.
        /// </summary>
        private static List<NotifyCollectionChangedEventArgs> CaptureEvents(FlatRowCollection frc)
        {
            var events = new List<NotifyCollectionChangedEventArgs>();
            frc.CollectionChanged += (_, e) => events.Add(e);
            return events;
        }

        // ------------------------------------------------------------------ //
        //  Task 3.2 — Count                                                  //
        // ------------------------------------------------------------------ //

        [Test]
        public void Count_NoGroupsExpanded_EqualsGroupCount()
        {
            var source = MakeSource(groupCount: 4, itemsPerGroup: 10);
            var frc = new FlatRowCollection(source.Object);

            Assert.That(frc.Count, Is.EqualTo(4));
        }

        [Test]
        public void Count_ZeroGroups_IsZero()
        {
            var source = MakeSource(groupCount: 0);
            var frc = new FlatRowCollection(source.Object);

            Assert.That(frc.Count, Is.EqualTo(0));
        }

        [Test]
        public void Count_AfterExpand_IncludesGroupItemsAndHeaders()
        {
            var source = MakeSource(groupCount: 3, itemsPerGroup: 5);
            var frc = new FlatRowCollection(source.Object);

            frc.SetExpanded(1, true);

            // 3 headers + 5 items from group 1
            Assert.That(frc.Count, Is.EqualTo(3 + 5));
        }

        [Test]
        public void Count_AfterExpandThenCollapse_ReturnsToPreviousCount()
        {
            var source = MakeSource(groupCount: 3, itemsPerGroup: 5);
            var frc = new FlatRowCollection(source.Object);

            frc.SetExpanded(0, true);
            frc.SetExpanded(0, false);

            Assert.That(frc.Count, Is.EqualTo(3));
        }

        // ------------------------------------------------------------------ //
        //  Task 3.2 — Indexer                                                //
        // ------------------------------------------------------------------ //

        [Test]
        public void Indexer_ReturnsGroupRow_AtGroupOffset()
        {
            var source = MakeSource(groupCount: 3, itemsPerGroup: 0);
            var expectedGroup = new GridGroupRow { Label = "G1", IsEnabled = true };
            source.Setup(s => s.GetGroup(1)).Returns(expectedGroup);
            var frc = new FlatRowCollection(source.Object);

            var row = frc[1];

            Assert.That(row, Is.SameAs(expectedGroup));
        }

        [Test]
        public void Indexer_ReturnsFirstItemRow_AfterGroupHeader()
        {
            var source = MakeSource(groupCount: 2, itemsPerGroup: 3);
            var expectedItem = new GridItemRow();
            source.Setup(s => s.GetItem(0, 0)).Returns(expectedItem);
            var frc = new FlatRowCollection(source.Object);

            frc.SetExpanded(0, true);

            // flat: [G0, I0_0, I0_1, I0_2, G1]  =>  index 1 is first item of G0
            var row = frc[1];
            Assert.That(row, Is.SameAs(expectedItem));
        }

        [Test]
        public void Indexer_ReturnsCorrectItemRow_InSecondExpandedGroup()
        {
            var source = MakeSource(groupCount: 3, itemsPerGroup: 2);
            var targetItem = new GridItemRow();
            source.Setup(s => s.GetItem(2, 1)).Returns(targetItem);
            var frc = new FlatRowCollection(source.Object);

            // Expand groups 1 and 2: flat = [G0, G1, I1_0, I1_1, G2, I2_0, I2_1]
            frc.SetExpanded(1, true);
            frc.SetExpanded(2, true);

            var row = frc[6]; // last item
            Assert.That(row, Is.SameAs(targetItem));
        }

        [Test]
        public void Indexer_OutOfRange_Throws()
        {
            var source = MakeSource(groupCount: 2, itemsPerGroup: 0);
            var frc = new FlatRowCollection(source.Object);

            Assert.Throws<ArgumentOutOfRangeException>(() => { var _ = frc[2]; });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var _ = frc[-1]; });
        }

        // ------------------------------------------------------------------ //
        //  Task 3.3 — Expand fires CollectionChanged                         //
        // ------------------------------------------------------------------ //

        [Test]
        public void SetExpanded_True_FiresAddEvent()
        {
            var source = MakeSource(groupCount: 2, itemsPerGroup: 3);
            var frc = new FlatRowCollection(source.Object);
            var events = CaptureEvents(frc);

            frc.SetExpanded(0, true);

            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
            Assert.That(events[0].NewStartingIndex, Is.EqualTo(1)); // after G0 header
            Assert.That(events[0].NewItems.Count, Is.EqualTo(3));
        }

        [Test]
        public void SetExpanded_True_AddEvent_StartsAfterCorrectGroupHeader()
        {
            // Expand the second group; its insert position must account for G0 header.
            var source = MakeSource(groupCount: 3, itemsPerGroup: 2);
            var frc = new FlatRowCollection(source.Object);
            var events = CaptureEvents(frc);

            frc.SetExpanded(1, true);

            Assert.That(events[0].NewStartingIndex, Is.EqualTo(2)); // flat: [G0, G1, ...]
        }

        [Test]
        public void SetExpanded_False_FiresRemoveEvent()
        {
            var source = MakeSource(groupCount: 2, itemsPerGroup: 3);
            var frc = new FlatRowCollection(source.Object);
            frc.SetExpanded(0, true);
            var events = CaptureEvents(frc);

            frc.SetExpanded(0, false);

            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));
            Assert.That(events[0].OldStartingIndex, Is.EqualTo(1));
            Assert.That(events[0].OldItems.Count, Is.EqualTo(3));
        }

        [Test]
        public void SetExpanded_True_OnAlreadyExpanded_IsNoOp()
        {
            var source = MakeSource(groupCount: 1, itemsPerGroup: 2);
            var frc = new FlatRowCollection(source.Object);
            frc.SetExpanded(0, true);
            var events = CaptureEvents(frc);

            frc.SetExpanded(0, true); // second call

            Assert.That(events, Is.Empty);
            Assert.That(frc.Count, Is.EqualTo(1 + 2));
        }

        [Test]
        public void SetExpanded_False_OnCollapsed_IsNoOp()
        {
            var source = MakeSource(groupCount: 1, itemsPerGroup: 2);
            var frc = new FlatRowCollection(source.Object);
            var events = CaptureEvents(frc);

            frc.SetExpanded(0, false); // already collapsed

            Assert.That(events, Is.Empty);
        }

        [Test]
        public void SetExpanded_ExpandZeroItemGroup_NoAddEventFired()
        {
            var source = MakeSource(groupCount: 2, itemsPerGroup: 0);
            var frc = new FlatRowCollection(source.Object);
            var events = CaptureEvents(frc);

            frc.SetExpanded(0, true);

            Assert.That(events, Is.Empty);
            Assert.That(frc.Count, Is.EqualTo(2));
        }

        // ------------------------------------------------------------------ //
        //  Task 3.3 — FlatOffset consistency across multiple expand/collapse //
        // ------------------------------------------------------------------ //

        [Test]
        public void Indexer_AfterExpandFirstGroup_GroupHeadersShiftCorrectly()
        {
            var source = MakeSource(groupCount: 3, itemsPerGroup: 2);
            var g2 = new GridGroupRow { Label = "G2", IsEnabled = true };
            source.Setup(s => s.GetGroup(2)).Returns(g2);
            var frc = new FlatRowCollection(source.Object);

            // flat before: [G0, G1, G2]
            // flat after expand(0): [G0, I0_0, I0_1, G1, G2]
            frc.SetExpanded(0, true);

            Assert.That(frc[3], Is.SameAs(source.Object.GetGroup(1)));
            Assert.That(frc[4], Is.SameAs(g2));
        }

        // ------------------------------------------------------------------ //
        //  Task 3.3 — Disabled group cannot expand                           //
        // ------------------------------------------------------------------ //

        [Test]
        public void SetExpanded_DisabledGroup_IsNoOp()
        {
            var source = MakeSource(groupCount: 2, itemsPerGroup: 3);
            var disabledGroup = new GridGroupRow { Label = "G0", IsEnabled = false };
            source.Setup(s => s.GetGroup(0)).Returns(disabledGroup);
            var frc = new FlatRowCollection(source.Object);
            var events = CaptureEvents(frc);

            frc.SetExpanded(0, true);

            Assert.That(events, Is.Empty);
            Assert.That(frc.Count, Is.EqualTo(2)); // no items added
        }

        // ------------------------------------------------------------------ //
        //  Task 3.4 — SingleExpandMode                                       //
        // ------------------------------------------------------------------ //

        [Test]
        public void SingleExpandMode_ExpandSecondGroup_CollapsesFirst()
        {
            var source = MakeSource(groupCount: 3, itemsPerGroup: 2);
            var frc = new FlatRowCollection(source.Object);
            frc.SingleExpandMode = true;

            frc.SetExpanded(0, true);
            var events = CaptureEvents(frc);

            frc.SetExpanded(1, true);

            // Should have fired: Remove for G0's items, then Add for G1's items
            Assert.That(events.Count, Is.EqualTo(2));
            Assert.That(events[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));
            Assert.That(events[1].Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
            // Only G1's items visible now
            Assert.That(frc.Count, Is.EqualTo(3 + 2));
        }

        [Test]
        public void SingleExpandMode_ExpandSameGroupTwice_IsNoOp()
        {
            var source = MakeSource(groupCount: 2, itemsPerGroup: 2);
            var frc = new FlatRowCollection(source.Object);
            frc.SingleExpandMode = true;

            frc.SetExpanded(0, true);
            var events = CaptureEvents(frc);

            frc.SetExpanded(0, true); // same group again

            Assert.That(events, Is.Empty);
        }

        [Test]
        public void SingleExpandMode_CollapseHint_CalledAfterPreviousGroupCollapses()
        {
            var source = MakeSource(groupCount: 2, itemsPerGroup: 1);
            var hint = source.As<IGridCollapseHint>();
            hint.Setup(h => h.OnGroupCollapsed(It.IsAny<int>()));
            var frc = new FlatRowCollection(source.Object);
            frc.SingleExpandMode = true;

            frc.SetExpanded(0, true);
            frc.SetExpanded(1, true);

            hint.Verify(h => h.OnGroupCollapsed(0), Times.Once);
        }

        [Test]
        public void SingleExpandMode_CollapseHint_NotCalledWhenNoPreviousExpanded()
        {
            var source = MakeSource(groupCount: 2, itemsPerGroup: 1);
            var hint = source.As<IGridCollapseHint>();
            hint.Setup(h => h.OnGroupCollapsed(It.IsAny<int>()));
            var frc = new FlatRowCollection(source.Object);
            frc.SingleExpandMode = true;

            frc.SetExpanded(0, true);

            hint.Verify(h => h.OnGroupCollapsed(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public void SingleExpandMode_SourceDoesNotImplementHint_NoException()
        {
            // MakeSource() mock does NOT implement IGridCollapseHint by default
            var source = MakeSource(groupCount: 2, itemsPerGroup: 1);
            var frc = new FlatRowCollection(source.Object);
            frc.SingleExpandMode = true;

            frc.SetExpanded(0, true);
            Assert.DoesNotThrow(() => frc.SetExpanded(1, true));
        }

        // ------------------------------------------------------------------ //
        //  Task 3.4 — EnforceSingleExpandMode                                //
        // ------------------------------------------------------------------ //

        [Test]
        public void EnforceSingleExpandMode_CollapsesAllButFirst()
        {
            var source = MakeSource(groupCount: 4, itemsPerGroup: 2);
            var frc = new FlatRowCollection(source.Object);

            // Manually expand three groups before turning on the mode
            frc.SetExpanded(0, true);
            frc.SetExpanded(2, true);
            frc.SetExpanded(3, true);

            var events = CaptureEvents(frc);
            frc.SingleExpandMode = true; // triggers EnforceSingleExpandMode

            // Only groups 2 and 3 should have been collapsed (not group 0)
            Assert.That(events.Count, Is.EqualTo(2));
            Assert.That(events[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));
            Assert.That(events[1].Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            // Group 0 still expanded: 4 headers + 2 items
            Assert.That(frc.Count, Is.EqualTo(4 + 2));
        }

        [Test]
        public void EnforceSingleExpandMode_NoGroupsExpanded_IsNoOp()
        {
            var source = MakeSource(groupCount: 3, itemsPerGroup: 2);
            var frc = new FlatRowCollection(source.Object);
            var events = CaptureEvents(frc);

            frc.SingleExpandMode = true;

            Assert.That(events, Is.Empty);
        }

        // ------------------------------------------------------------------ //
        //  Task 3.5 — DataReset                                              //
        // ------------------------------------------------------------------ //

        [Test]
        public void DataReset_RaisesResetCollectionChanged()
        {
            var source = MakeSource(groupCount: 3, itemsPerGroup: 2);
            EventHandler<GroupChangedEventArgs> groupChangedDelegate = null;
            EventHandler<ItemChangedEventArgs> itemChangedDelegate = null;
            EventHandler dataResetDelegate = null;

            source.SetupAdd(s => s.GroupChanged += It.IsAny<EventHandler<GroupChangedEventArgs>>())
                  .Callback<EventHandler<GroupChangedEventArgs>>(h => groupChangedDelegate = h);
            source.SetupAdd(s => s.ItemChanged += It.IsAny<EventHandler<ItemChangedEventArgs>>())
                  .Callback<EventHandler<ItemChangedEventArgs>>(h => itemChangedDelegate = h);
            source.SetupAdd(s => s.DataReset += It.IsAny<EventHandler>())
                  .Callback<EventHandler>(h => dataResetDelegate = h);

            var frc = new FlatRowCollection(source.Object);
            frc.SetExpanded(0, true);
            var events = CaptureEvents(frc);

            // Simulate source firing DataReset
            dataResetDelegate?.Invoke(source.Object, EventArgs.Empty);

            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
            // Collection should be rebuilt collapsed
            Assert.That(frc.Count, Is.EqualTo(3));
        }

        // ------------------------------------------------------------------ //
        //  Task 3.5 — GroupChanged                                           //
        // ------------------------------------------------------------------ //

        [Test]
        public void GroupChanged_Added_FiresAddAndAdjustsCount()
        {
            var source = MakeSource(groupCount: 2, itemsPerGroup: 0);
            EventHandler<GroupChangedEventArgs> groupChangedDelegate = null;
            source.SetupAdd(s => s.GroupChanged += It.IsAny<EventHandler<GroupChangedEventArgs>>())
                  .Callback<EventHandler<GroupChangedEventArgs>>(h => groupChangedDelegate = h);
            source.SetupAdd(s => s.ItemChanged += It.IsAny<EventHandler<ItemChangedEventArgs>>());
            source.SetupAdd(s => s.DataReset += It.IsAny<EventHandler>());

            var frc = new FlatRowCollection(source.Object);

            // Source will now have 3 groups after the Add
            var newGroup = new GridGroupRow { Label = "G2", IsEnabled = true };
            source.Setup(s => s.GroupCount).Returns(3);
            source.Setup(s => s.GetGroup(2)).Returns(newGroup);
            source.Setup(s => s.GetItemCount(2)).Returns(0);

            var events = CaptureEvents(frc);
            groupChangedDelegate?.Invoke(source.Object, new GroupChangedEventArgs(2, GroupChangeKind.Added));

            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
            Assert.That(frc.Count, Is.EqualTo(3));
        }

        [Test]
        public void GroupChanged_Removed_FiresRemoveAndAdjustsCount()
        {
            var source = MakeSource(groupCount: 3, itemsPerGroup: 0);
            EventHandler<GroupChangedEventArgs> groupChangedDelegate = null;
            source.SetupAdd(s => s.GroupChanged += It.IsAny<EventHandler<GroupChangedEventArgs>>())
                  .Callback<EventHandler<GroupChangedEventArgs>>(h => groupChangedDelegate = h);
            source.SetupAdd(s => s.ItemChanged += It.IsAny<EventHandler<ItemChangedEventArgs>>());
            source.SetupAdd(s => s.DataReset += It.IsAny<EventHandler>());

            var frc = new FlatRowCollection(source.Object);

            // Simulate group 1 being removed
            source.Setup(s => s.GroupCount).Returns(2);

            var events = CaptureEvents(frc);
            groupChangedDelegate?.Invoke(source.Object, new GroupChangedEventArgs(1, GroupChangeKind.Removed));

            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));
            Assert.That(frc.Count, Is.EqualTo(2));
        }

        [Test]
        public void GroupChanged_Updated_FiresReplaceAtGroupOffset()
        {
            var source = MakeSource(groupCount: 3, itemsPerGroup: 0);
            EventHandler<GroupChangedEventArgs> groupChangedDelegate = null;
            source.SetupAdd(s => s.GroupChanged += It.IsAny<EventHandler<GroupChangedEventArgs>>())
                  .Callback<EventHandler<GroupChangedEventArgs>>(h => groupChangedDelegate = h);
            source.SetupAdd(s => s.ItemChanged += It.IsAny<EventHandler<ItemChangedEventArgs>>());
            source.SetupAdd(s => s.DataReset += It.IsAny<EventHandler>());

            var frc = new FlatRowCollection(source.Object);
            var events = CaptureEvents(frc);

            groupChangedDelegate?.Invoke(source.Object, new GroupChangedEventArgs(1, GroupChangeKind.Updated));

            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Replace));
            Assert.That(events[0].NewStartingIndex, Is.EqualTo(1)); // group 1 is at flat offset 1
        }

        // ------------------------------------------------------------------ //
        //  Task 3.5 — ItemChanged (expanded group)                           //
        // ------------------------------------------------------------------ //

        [Test]
        public void ItemChanged_Added_ToExpandedGroup_FiresAddAndAdjustsCount()
        {
            var source = MakeSource(groupCount: 2, itemsPerGroup: 2);
            EventHandler<ItemChangedEventArgs> itemChangedDelegate = null;
            source.SetupAdd(s => s.GroupChanged += It.IsAny<EventHandler<GroupChangedEventArgs>>());
            source.SetupAdd(s => s.ItemChanged += It.IsAny<EventHandler<ItemChangedEventArgs>>())
                  .Callback<EventHandler<ItemChangedEventArgs>>(h => itemChangedDelegate = h);
            source.SetupAdd(s => s.DataReset += It.IsAny<EventHandler>());

            var newItem = new GridItemRow();
            source.SetupSequence(s => s.GetItemCount(0))
                  .Returns(2)  // during initial SetExpanded
                  .Returns(3); // after add
            source.Setup(s => s.GetItem(0, 2)).Returns(newItem);

            var frc = new FlatRowCollection(source.Object);
            frc.SetExpanded(0, true);
            var events = CaptureEvents(frc);

            itemChangedDelegate?.Invoke(source.Object, new ItemChangedEventArgs(0, 2, ItemChangeKind.Added));

            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
            Assert.That(frc.Count, Is.EqualTo(2 + 3)); // 2 headers + 3 items in group 0
        }

        [Test]
        public void ItemChanged_Removed_FromExpandedGroup_FiresRemoveAndAdjustsCount()
        {
            var source = MakeSource(groupCount: 2, itemsPerGroup: 3);
            EventHandler<ItemChangedEventArgs> itemChangedDelegate = null;
            source.SetupAdd(s => s.GroupChanged += It.IsAny<EventHandler<GroupChangedEventArgs>>());
            source.SetupAdd(s => s.ItemChanged += It.IsAny<EventHandler<ItemChangedEventArgs>>())
                  .Callback<EventHandler<ItemChangedEventArgs>>(h => itemChangedDelegate = h);
            source.SetupAdd(s => s.DataReset += It.IsAny<EventHandler>());

            var frc = new FlatRowCollection(source.Object);
            frc.SetExpanded(0, true);
            var events = CaptureEvents(frc);

            itemChangedDelegate?.Invoke(source.Object, new ItemChangedEventArgs(0, 1, ItemChangeKind.Removed));

            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));
            Assert.That(frc.Count, Is.EqualTo(2 + 2)); // 3 items minus 1
        }

        [Test]
        public void ItemChanged_ToCollapsedGroup_IsNoOp()
        {
            var source = MakeSource(groupCount: 2, itemsPerGroup: 3);
            EventHandler<ItemChangedEventArgs> itemChangedDelegate = null;
            source.SetupAdd(s => s.GroupChanged += It.IsAny<EventHandler<GroupChangedEventArgs>>());
            source.SetupAdd(s => s.ItemChanged += It.IsAny<EventHandler<ItemChangedEventArgs>>())
                  .Callback<EventHandler<ItemChangedEventArgs>>(h => itemChangedDelegate = h);
            source.SetupAdd(s => s.DataReset += It.IsAny<EventHandler>());

            var frc = new FlatRowCollection(source.Object);
            // Group 0 is NOT expanded
            var events = CaptureEvents(frc);

            itemChangedDelegate?.Invoke(source.Object, new ItemChangedEventArgs(0, 1, ItemChangeKind.Removed));

            Assert.That(events, Is.Empty);
            Assert.That(frc.Count, Is.EqualTo(2));
        }
    }
}
