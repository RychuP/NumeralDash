//using NumeralDash.Entities;
//using SadRogue.Primitives;

//namespace NumeralDash.Rules
//{
//    class UpAndDownOrder : CollectionRuleBase, ICollectionRule
//    {
//        public string Title => "Up & Down Order";
//        bool _lastNumberWasHigh = true;

//        public UpAndDownOrder(int count) : base(count)
//        {
//            Color = Color.DarkOrange;

//            // populate reversed numbers
//            for (int i = NumberCount; i >= 1; i--)
//            {
//                var n = new Number(i);
//                RemainingNumbers.Add(n);
//                Numbers[i - 1] = n;
//            }

//            // set the number to find
//            Dequeue();
//        }

//        public override void Dequeue()
//        {
//            if (RemainingNumbers.Count > 1)
//            {
//                if (_lastNumberWasHigh)
//                {
//                    SetNextAndRemove(0);
//                    _lastNumberWasHigh = false;
//                }
//                else
//                {
//                    SetNextAndRemove(RemainingNumbers.Count - 1);
//                    _lastNumberWasHigh = true;
//                }
//            }
//            else if (RemainingNumbers.Count == 1)
//            {
//                NextNumber = RemainingNumbers[0];
//                RemainingNumbers.RemoveAt(0);
//            }
//            else
//            {
//                NextNumber = Number.Finished;
//            }
//        }
//    }
//}
