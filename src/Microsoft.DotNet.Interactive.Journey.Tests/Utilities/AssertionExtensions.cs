using FluentAssertions;
using FluentAssertions.Collections;
using FluentAssertions.Primitives;
using Microsoft.DotNet.Interactive.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DotNet.Interactive.Journey.Tests.Utilities
{
    public static class AssertionExtensions
    {
        public static AndWhichConstraint<ObjectAssertions, T> ContainSingle<T>(
            this GenericCollectionAssertions<KernelEvent> should,
            Func<T, bool>? where = null)
            where T : KernelEvent
        {
            T subject;

            where ??= (_ => true);

            if (where is null)
            {
                should.ContainSingle(e => e is T);

                subject = should.Subject
                                .OfType<T>()
                                .Single();
            }
            else
            {
                should.ContainSingle(e => e is T && where((T)e));

                subject = should.Subject
                                .OfType<T>()
                                .Where(where)
                                .Single();
            }

            return new AndWhichConstraint<ObjectAssertions, T>(subject.Should(), subject);
        }
    }
}
