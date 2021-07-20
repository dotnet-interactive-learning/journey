using System.Threading.Tasks;

namespace Extension
{
    public interface ILessonHost
    {
        public Task StartChallengeAsync(Challenge challenge);
    }
}