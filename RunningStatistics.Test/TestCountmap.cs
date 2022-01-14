using System;
using Xunit;

namespace RunningStatistics.Test
{
    public class TestCountmap
    {
        [Fact]
        public void EmptyCountmapIsZero()
        {
            Countmap<string> countmap = new();

            Assert.Equal(0, countmap.Count);
            Assert.Equal(0, countmap["nothing"]);
        }

        [Fact]
        public void MergeEmptyIsZero()
        {
            Countmap<string> a, b;
            a = new(); b = new();

            Assert.Equal(0, b["everything"]);

            a.Merge(b);

            Assert.Equal(0, a.Count);
            Assert.Equal(0, a["nothing"]);
        }

        [Fact]
        public void MergingAddsEmptyKeys()
        {
            Countmap<string> a, b;
            a = new(); b = new();

            Assert.Equal(0, b["everything"]);

            a.Merge(b);

            Assert.True(a.Value.ContainsKey("everything"));
        }

        [Fact]
        public void MergingTwoCountmaps()
        {
            Countmap<string> a, b;
            a = new(); b = new();

            a.Fit("something", 5);
            b.Fit("nothing", 2);

            a.Merge(b);

            Assert.True(a.Value.ContainsKey("something"));
            Assert.True(a.Value.ContainsKey("nothing"));
            Assert.Equal(5, a["something"]);
            Assert.Equal(2, a["nothing"]);
        }
    }
}
