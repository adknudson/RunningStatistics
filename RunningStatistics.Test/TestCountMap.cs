using Xunit;

namespace RunningStatistics.Test
{
    public class TestCountMap
    {
        [Fact]
        public void AccessingNonExistentKeyReturnsZero()
        {
            CountMap<string> countMap = new();
            Assert.Equal(0, countMap["nothing"]);
        }

        [Fact]
        public void AccessingNonExistentKeyDoesNotAddKey()
        {
            CountMap<string> countMap = new();
            Assert.Equal(0, countMap["everything"]);
            Assert.False(countMap.ContainsKey("everything"));
        }

        [Fact]
        public void MergingTwoCountmapsMergesKeys()
        {
            CountMap<string> a = new(); 
            CountMap<string> b = new();

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
            CountMap<string> a = new();

            a.Fit("something", 1);
            a.Fit("everything", 2);
            
            Assert.NotEmpty(a.Value);
            a.Reset();
            Assert.Empty(a.Value);
            Assert.Equal(0, a.Nobs);
        }

        [Fact]
        public void StaticMergeDoesNotAffectOriginals()
        {
            CountMap<int> a = new();
            a.Fit(new [] {1, 1, 2, 3, 5, 8});

            CountMap<int> b = new();
            b.Fit(new []{4, 5, 8, 11});

            var c = CountMap<int>.Merge(a, b);
            Assert.Equal(a.Nobs + b.Nobs, c.Nobs);
            
            c.Fit(1);

            Assert.Equal(2, a[1]);
            Assert.Equal(1, a[8]);
            
            Assert.Equal(1, b[4]);
            Assert.Equal(1, b[5]);
            
            Assert.Equal(3, c[1]);
            Assert.Equal(2, c[5]);
            Assert.Equal(2, c[8]);
        }
    }
}
