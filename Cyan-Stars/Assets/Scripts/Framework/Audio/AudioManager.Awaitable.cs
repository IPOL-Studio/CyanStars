using System.Threading.Tasks;

namespace CyanStars.Framework.Audio
{
    public partial class AudioManager
    {
        /// <summary>
        /// 播放音乐（可等待）
        /// </summary>
        public Task<AudioAgent> PlayMusicAsync(string musicName,float volume = 1f,bool loop = true)
        {
            TaskCompletionSource<AudioAgent> tcs = new TaskCompletionSource<AudioAgent>();
            PlayMusicAsync(musicName,(agent =>
            {
                tcs.SetResult(agent);
            }),volume,loop);
            return tcs.Task;
        }
    }
}
