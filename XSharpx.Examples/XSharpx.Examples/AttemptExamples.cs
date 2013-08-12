using System;
using NUnit.Framework;
using XSharpx;

namespace XSharpx.Examples
{
    public class AttemptExamples
    {
        [Test]
        public void Select()
        {
            var r = new Attempt<int, int>(IsEven)
                      .Select(x => x.ToString ());

            Assert.AreEqual(Option.Some("2"), r.Run(2));
            Assert.IsEmpty(r.Run(3));
        }

        [Test]
        public void SelectMany() {
            var r = new Attempt<int,int>(IsEven).SelectMany (i => new Attempt<int, string>(DescribeIfSingleDigit));
            Assert.AreEqual(Option.Some("two"), r.Run (2));
            Assert.IsEmpty (r.Run (3));
            Assert.IsEmpty (r.Run (12));
        }

        [Test]
        public void SelectManyLinq() {
            var r =
                from a in new Attempt<int, int>(IsEven)
                from b in new Attempt<int, string>(DescribeIfSingleDigit)
                select b;

            Assert.AreEqual(Option.Some("two"), r.Run (2));
            Assert.IsEmpty (r.Run (3));
            Assert.IsEmpty (r.Run (12));
        }

        private Option<int> IsEven(int i) {
            return i%2==0 ? Option.Some(i) : Option.Empty;
        }

        private Option<string> DescribeIfSingleDigit(int i) {
            if (i==0) return Option.Some("zero");
            if (i==1) return Option.Some("one");
            if (i==2) return Option.Some("two");
            if (i==3) return Option.Some("three");
            if (i==4) return Option.Some("four");
            if (i==5) return Option.Some("five");
            if (i==6) return Option.Some("six");
            if (i==7) return Option.Some("seven");
            if (i==8) return Option.Some("eight");
            if (i==9) return Option.Some("nine");
            return Option.Empty;
        }
    }
}

