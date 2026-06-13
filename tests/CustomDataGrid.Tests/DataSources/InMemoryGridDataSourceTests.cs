using System;
using System.Collections.Generic;
using CustomDataGrid.Contracts;
using CustomDataGrid.Contracts.Events;
using CustomDataGrid.DataSources;
using CustomDataGrid.Models;
using NUnit.Framework;

namespace CustomDataGrid.Tests.DataSources
{
    [TestFixture]
    public class InMemoryGridDataSourceTests
    {
        // ------------------------------------------------------------------ //
        //  Helpers                                                            //
        // ------------------------------------------------------------------ //

        private static GridGroupRow MakeGroup(string label, int itemCount)
        {
            var group = new GridGroupRow { Label = label };
            for (int i = 0; i < itemCount; i++)
                group.Items.Add(new GridItemRow());
            return group;
        }

        // ------------------------------------------------------------------ //
        //  Basic delegation                                                   //
        // ------------------------------------------------------------------ //

        [Test]
        public void GroupCount_ReflectsConstructorGroups()
        {
            var source = new InMemoryGridDataSource(new List<GridGroupRow>
            {
                MakeGroup("G0", 2),
                MakeGroup("G1", 3),
            });

            Assert.That(source.GroupCount, Is.EqualTo(2));
        }

        [Test]
        public void GetItemCount_ReturnsGroupItemsCount()
        {
            var source = new InMemoryGridDataSource(new List<GridGroupRow>
            {
                MakeGroup("G0", 5),
            });

            Assert.That(source.GetItemCount(0), Is.EqualTo(5));
        }

        [Test]
        public void GetGroup_ReturnsGroupAtIndex()
        {
            var g0 = MakeGroup("G0", 0);
            var g1 = MakeGroup("G1", 0);
            var source = new InMemoryGridDataSource(new List<GridGroupRow> { g0, g1 });

            Assert.That(source.GetGroup(0), Is.SameAs(g0));
            Assert.That(source.GetGroup(1), Is.SameAs(g1));
        }

        [Test]
        public void GetItem_ReturnsItemAtIndex()
        {
            var group = MakeGroup("G0", 3);
            var source = new InMemoryGridDataSource(new List<GridGroupRow> { group });

            Assert.That(source.GetItem(0, 1), Is.SameAs(group.Items[1]));
        }

        [Test]
        public void GetItems_ReturnsRequestedRange()
        {
            var group = MakeGroup("G0", 5);
            var source = new InMemoryGridDataSource(new List<GridGroupRow> { group });

            var range = source.GetItems(0, 1, 3);

            Assert.That(range.Count, Is.EqualTo(3));
            Assert.That(range[0], Is.SameAs(group.Items[1]));
            Assert.That(range[1], Is.SameAs(group.Items[2]));
            Assert.That(range[2], Is.SameAs(group.Items[3]));
        }

        [Test]
        public void GetItems_RangeExceedingBounds_ReturnsTruncatedList()
        {
            var group = MakeGroup("G0", 3);
            var source = new InMemoryGridDataSource(new List<GridGroupRow> { group });

            var range = source.GetItems(0, 1, 10);

            Assert.That(range.Count, Is.EqualTo(2));
        }

        // ------------------------------------------------------------------ //
        //  AddGroup / RemoveGroup / UpdateGroup                               //
        // ------------------------------------------------------------------ //

        [Test]
        public void AddGroup_InsertsGroupAndRaisesAdded()
        {
            var source = new InMemoryGridDataSource(new List<GridGroupRow> { MakeGroup("G0", 0) });
            GroupChangedEventArgs received = null;
            source.GroupChanged += (s, e) => received = e;

            var newGroup = MakeGroup("G1", 0);
            source.AddGroup(1, newGroup);

            Assert.That(source.GroupCount, Is.EqualTo(2));
            Assert.That(source.GetGroup(1), Is.SameAs(newGroup));
            Assert.That(received.GroupIndex, Is.EqualTo(1));
            Assert.That(received.Kind, Is.EqualTo(GroupChangeKind.Added));
        }

        [Test]
        public void RemoveGroup_RemovesGroupAndRaisesRemoved()
        {
            var source = new InMemoryGridDataSource(new List<GridGroupRow>
            {
                MakeGroup("G0", 0),
                MakeGroup("G1", 0),
            });
            GroupChangedEventArgs received = null;
            source.GroupChanged += (s, e) => received = e;

            source.RemoveGroup(0);

            Assert.That(source.GroupCount, Is.EqualTo(1));
            Assert.That(source.GetGroup(0).Label, Is.EqualTo("G1"));
            Assert.That(received.GroupIndex, Is.EqualTo(0));
            Assert.That(received.Kind, Is.EqualTo(GroupChangeKind.Removed));
        }

        [Test]
        public void UpdateGroup_RaisesUpdatedWithoutChangingCount()
        {
            var source = new InMemoryGridDataSource(new List<GridGroupRow> { MakeGroup("G0", 0) });
            GroupChangedEventArgs received = null;
            source.GroupChanged += (s, e) => received = e;

            source.UpdateGroup(0);

            Assert.That(source.GroupCount, Is.EqualTo(1));
            Assert.That(received.GroupIndex, Is.EqualTo(0));
            Assert.That(received.Kind, Is.EqualTo(GroupChangeKind.Updated));
        }

        // ------------------------------------------------------------------ //
        //  AddItem / RemoveItem / UpdateItem                                  //
        // ------------------------------------------------------------------ //

        [Test]
        public void AddItem_InsertsItemAndRaisesAdded()
        {
            var source = new InMemoryGridDataSource(new List<GridGroupRow> { MakeGroup("G0", 2) });
            ItemChangedEventArgs received = null;
            source.ItemChanged += (s, e) => received = e;

            var newItem = new GridItemRow();
            source.AddItem(0, 1, newItem);

            Assert.That(source.GetItemCount(0), Is.EqualTo(3));
            Assert.That(source.GetItem(0, 1), Is.SameAs(newItem));
            Assert.That(received.GroupIndex, Is.EqualTo(0));
            Assert.That(received.ItemIndex, Is.EqualTo(1));
            Assert.That(received.Kind, Is.EqualTo(ItemChangeKind.Added));
        }

        [Test]
        public void RemoveItem_RemovesItemAndRaisesRemoved()
        {
            var group = MakeGroup("G0", 3);
            var remaining = group.Items[2];
            var source = new InMemoryGridDataSource(new List<GridGroupRow> { group });
            ItemChangedEventArgs received = null;
            source.ItemChanged += (s, e) => received = e;

            source.RemoveItem(0, 0);

            Assert.That(source.GetItemCount(0), Is.EqualTo(2));
            Assert.That(source.GetItem(0, 1), Is.SameAs(remaining));
            Assert.That(received.GroupIndex, Is.EqualTo(0));
            Assert.That(received.ItemIndex, Is.EqualTo(0));
            Assert.That(received.Kind, Is.EqualTo(ItemChangeKind.Removed));
        }

        [Test]
        public void UpdateItem_RaisesUpdatedWithoutChangingCount()
        {
            var source = new InMemoryGridDataSource(new List<GridGroupRow> { MakeGroup("G0", 2) });
            ItemChangedEventArgs received = null;
            source.ItemChanged += (s, e) => received = e;

            source.UpdateItem(0, 1);

            Assert.That(source.GetItemCount(0), Is.EqualTo(2));
            Assert.That(received.GroupIndex, Is.EqualTo(0));
            Assert.That(received.ItemIndex, Is.EqualTo(1));
            Assert.That(received.Kind, Is.EqualTo(ItemChangeKind.Updated));
        }

        // ------------------------------------------------------------------ //
        //  ReplaceGroups / DataReset                                          //
        // ------------------------------------------------------------------ //

        [Test]
        public void ReplaceGroups_ReplacesGroupsAndRaisesDataReset()
        {
            var source = new InMemoryGridDataSource(new List<GridGroupRow> { MakeGroup("G0", 0) });
            int resetCount = 0;
            source.DataReset += (s, e) => resetCount++;

            var newGroups = new List<GridGroupRow> { MakeGroup("X0", 0), MakeGroup("X1", 0) };
            source.ReplaceGroups(newGroups);

            Assert.That(source.GroupCount, Is.EqualTo(2));
            Assert.That(source.GetGroup(0).Label, Is.EqualTo("X0"));
            Assert.That(resetCount, Is.EqualTo(1));
        }

        // ------------------------------------------------------------------ //
        //  Constructor guard                                                  //
        // ------------------------------------------------------------------ //

        [Test]
        public void Constructor_NullGroups_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new InMemoryGridDataSource(null));
        }
    }
}
