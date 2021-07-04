using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Criterion
{
    public class QuestionTextCriterion
    {
        Predicate<string> predicate;

        public static QuestionTextCriterion FromPredicate(Predicate<string> predicate)
        {
            return new QuestionTextCriterion(predicate);
        }

        private QuestionTextCriterion(Predicate<string> predicate)
        {
            this.predicate = predicate;
        }

        public Predicate<string> ToPredicate()
        {
            return predicate;
        }
    }
}
