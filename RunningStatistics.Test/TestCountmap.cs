using System;
using Xunit;

namespace RunningStatistics.Test
{
    public class TestCountmap
    {
        [Fact]
        public void AccessingNonExistentKeyReturnsZero()
        {
            Countmap<string> countmap = new();

            Assert.Empty(countmap);
            Assert.Equal(0, countmap["nothing"]);
        }

        [Fact]
        public void AccessingNonExistentKeyDoesNotAddKey()
        {
            Countmap<string> countmap = new();
            Assert.Equal(0, countmap["everything"]);
            Assert.False(countmap.ContainsKey("everything"));
        }

        [Fact]
        public void MergingTwoCountmapsMergesKeys()
        {
            Countmap<string> a = new(); 
            Countmap<string> b = new();

            a.Fit("something", 1);
            a.Fit("everything", 2);
            
            b.Fit("nothing", 3);
            b.Fit("everything", 4);

            a.Merge(b);

            Assert.True(a.ContainsKey("something"));
            Assert.True(a.ContainsKey("nothing"));
            Assert.True(a.ContainsKey("everything"));
            
            Assert.Equal(1, a["something"]);
            Assert.Equal(3, a["nothing"]);
            Assert.Equal(6, a["everything"]);
        }

        [Fact]
        public void ResettingRemovesAllKeys()
        {
            Countmap<string> a = new();

            a.Fit("something", 1);
            a.Fit("everything", 2);
            
            Assert.NotEmpty(a);
            a.Reset();
            Assert.Empty(a);
        }

        [Fact]
        public void ModeReturnsMostObservedValue()
        {
            Countmap<int> a = new();
            var rng = new Random();

            for (var i = 0; i < 10; i++)
            {
                a.Fit(i, rng.Next(1, 11));
            }

            a.Fit(5, 20);
            
            Assert.Equal(5, a.Mode);
        }
    }
}
