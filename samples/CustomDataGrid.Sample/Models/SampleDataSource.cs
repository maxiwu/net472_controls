using System;
using System.Collections.Generic;
using System.Globalization;
using CustomDataGrid.DataSources;
using CustomDataGrid.Models;

namespace CustomDataGrid.Sample.Models
{
    /// <summary>
    /// The sample data source (Task 8.3). Builds ten groups of randomized items
    /// on top of <see cref="InMemoryGridDataSource"/>, seeding the specific
    /// disabled / highlighted rows the manual integration checks (Task 8.8)
    /// exercise.
    /// </summary>
    public class SampleDataSource : InMemoryGridDataSource
    {
        private int _nextGroupNumber;

        /// <summary>
        /// Initializes a new <see cref="SampleDataSource"/> with ten seeded
        /// groups.
        /// </summary>
        public SampleDataSource()
            : base(BuildGroups(out int produced))
        {
            _nextGroupNumber = produced + 1;
        }

        /// <summary>
        /// Creates a new, empty-but-seeded sample group with the next sequential
        /// label and a few random items, suitable for the header bar's
        /// "Add Group" action. Does not insert it into the source.
        /// </summary>
        public SampleGroupRow CreateGroup()
        {
            var rng = new Random();
            int number = _nextGroupNumber++;
            var group = NewGroup(number);
            int itemCount = rng.Next(8, 13);
            for (int i = 0; i < itemCount; i++)
                group.Items.Add(NewItem(rng));
            group.TotalItemCount = group.Items.Count;
            return group;
        }

        private static IList<GridGroupRow> BuildGroups(out int produced)
        {
            var rng = new Random(20260614);
            var groups = new List<GridGroupRow>();

            for (int g = 1; g <= 10; g++)
            {
                var group = NewGroup(g);

                int itemCount = rng.Next(8, 13); // 8..12 inclusive
                for (int i = 0; i < itemCount; i++)
                    group.Items.Add(NewItem(rng));

                group.TotalItemCount = group.Items.Count;
                groups.Add(group);
            }

            // Seeded states for the integration checks (Task 8.3 / 8.8).
            groups[2].IsEnabled = false;                 // Group 003 disabled
            if (groups[1].Items.Count > 4)
                groups[1].Items[4].IsEnabled = false;    // Group 002, item index 4 disabled
            if (groups[0].Items.Count > 2)
                groups[0].Items[2].IsHighlighted = true; // Group 001, item index 2 highlighted

            produced = 10;
            return groups;
        }

        private static SampleGroupRow NewGroup(int number)
        {
            string label = "Group " + number.ToString("D3", CultureInfo.InvariantCulture);
            return new SampleGroupRow
            {
                Label = label,
                Description = label + " description",
                Status = "Enable"
            };
        }

        private static SampleItemRow NewItem(Random rng)
        {
            return new SampleItemRow
            {
                X = (float)Math.Round(rng.NextDouble() * 100.0, 2),
                Y = (float)Math.Round(rng.NextDouble() * 100.0, 2)
            };
        }
    }
}
