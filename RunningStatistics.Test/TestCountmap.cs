using Xunit;

namespace RunningStatistics.Test
{
    public class TestCountmap
    {
        [Fact]
        public void EmptyCountmapIsZero()
        {
            Countmap<string> countmap = new();

            Assert.Empty(countmap);
            Assert.Equal(0, countmap["nothing"]);
        }

        [Fact]
        public void MergeEmptyIsZero()
        {
            Countmap<string> a = new(); Countmap<string> b = new();

            Assert.Equal(0, b["everything"]);

            a.Merge(b);

            Assert.Empty(a);
            Assert.Equal(0, a["nothing"]);
        }

        [Fact]
        public void AccessingNonFittedValueDoesntAddKey()
        {
            Countmap<string> a = new();
            Assert.Equal(0, a["everything"]);
            Assert.False(a.ContainsKey("everything"));
        }

        [Fact]
        public void MergingTwoCountmaps()
        {
            Countmap<string> a = new(); Countmap<string> b = new();

            a.Fit("something", 5);
            b.Fit("nothing", 2);

            a.Merge(b);

            Assert.True(a.ContainsKey("something"));
            Assert.True(a.ContainsKey("nothing"));
            Assert.Equal(5, a["something"]);
            Assert.Equal(2, a["nothing"]);
        }
    }
}
