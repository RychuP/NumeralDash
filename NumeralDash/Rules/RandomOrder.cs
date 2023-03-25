//using NumeralDash.Entities;

//namespace NumeralDash.Rules
//{
//    class RandomOrder : CollectionRuleBase
//    {
//        public RandomOrder(int count) : base(count, "Random Order", Color.LightSalmon)
//        {
//            // generate numbers
//            for (int i = 1; i <= count; i++)
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
//                int index = Program.GetRandomIndex(RemainingNumbers.Count);
//                SetNextAndRemove(index);
//            }
//            else if (RemainingNumbers.Count == 1)
//            {
//                SetNextAndRemove(0);
//            }
//            else
//            {
//                NextNumber = Number.Finished;
//            }
//        }
//    }
//}
