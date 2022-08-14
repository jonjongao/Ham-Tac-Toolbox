using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
#if DOTWEEN_INSTALLED
using DG.Tweening;
#endif

namespace HamTac
{
    public static class Tween
    {

        public static async Task MoveToPosition(Transform self, Vector3 position, float duraiton)
        {
#if DOTWEEN_INSTALLED
            self.DOMove(position, duraiton).SetEase(Ease.OutCubic);
            await Extension.Async.Delay(duraiton);
#endif
        }

    }
}