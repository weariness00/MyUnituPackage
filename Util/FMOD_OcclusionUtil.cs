using UnityEngine;

namespace Weariness.FMOD.Occlusion
{
    public static class FMOD_OcclusionUtil
    {
        public static float OccludeBetween(Vector3 sound, Vector3 listener, FMOD_OcclusionData occlusionData)
            => OccludeBetween(sound, listener, occlusionData.SoundOcclusionWidening, occlusionData.PlayerOcclusionWidening, occlusionData.OcclusionLayer);
        public static float OccludeBetween(Vector3 sound, Vector3 listener) => OccludeBetween(sound, listener, 1f, 1f, int.MaxValue);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sound"></param>
        /// <param name="listener"></param>
        /// <param name="soundOcclusionWidening"></param>
        /// <param name="playerOcclusionWidening"></param>
        /// <param name="occlusionLayer"></param>
        /// <returns>0~1 사이값으로 반환, 1 완전 차폐, 0 차폐 없음</returns>
        public static float OccludeBetween(Vector3 sound, Vector3 listener, float soundOcclusionWidening, float playerOcclusionWidening, LayerMask occlusionLayer)
        {
            Vector3 soundLeft = CalculatePoint(sound, listener, soundOcclusionWidening, true);
            Vector3 soundRight = CalculatePoint(sound, listener, soundOcclusionWidening, false);

            Vector3 soundAbove = new Vector3(sound.x, sound.y + soundOcclusionWidening, sound.z);
            Vector3 soundBelow = new Vector3(sound.x, sound.y - soundOcclusionWidening, sound.z);

            Vector3 listenerLeft = CalculatePoint(listener, sound, playerOcclusionWidening, true);
            Vector3 listenerRight = CalculatePoint(listener, sound, playerOcclusionWidening, false);

            Vector3 listenerAbove = new Vector3(listener.x, listener.y + playerOcclusionWidening * 0.5f, listener.z);
            Vector3 listenerBelow = new Vector3(listener.x, listener.y - playerOcclusionWidening * 0.5f, listener.z);

            int occlusionCount = 0;
            void CastLine(Vector3 start, Vector3 end)
            {
                RaycastHit hit;
                Physics.Linecast(start, end, out hit, occlusionLayer);
                if (hit.collider != null) occlusionCount++;
#if UNITY_EDITOR
                if (playerOcclusionWidening == 0f || soundOcclusionWidening == 0f)
                    Debug.DrawLine(start, end, Color.blue);
                else
                    Debug.DrawLine(start, end, hit.collider ? Color.red : Color.green);
#endif
            }
            CastLine(soundLeft, listenerLeft);
            CastLine(soundLeft, listener);
            CastLine(soundLeft, listenerRight);

            CastLine(sound, listenerLeft);
            CastLine(sound, listener);
            CastLine(sound, listenerRight);

            CastLine(soundRight, listenerLeft);
            CastLine(soundRight, listener);
            CastLine(soundRight, listenerRight);

            CastLine(soundAbove, listenerAbove);
            CastLine(soundBelow, listenerBelow);

            return occlusionCount / 11f;
        }

        public static Vector3 CalculatePoint(Vector3 a, Vector3 b, float m, bool posOrneg)
        {
            float x;
            float z;
            float n = Vector3.Distance(new Vector3(a.x, 0f, a.z), new Vector3(b.x, 0f, b.z));
            float mn = (m / n);
            if (posOrneg)
            {
                x = a.x + (mn * (a.z - b.z));
                z = a.z - (mn * (a.x - b.x));
            }
            else
            {
                x = a.x - (mn * (a.z - b.z));
                z = a.z + (mn * (a.x - b.x));
            }

            return new Vector3(x, a.y, z);
        }
    }
}