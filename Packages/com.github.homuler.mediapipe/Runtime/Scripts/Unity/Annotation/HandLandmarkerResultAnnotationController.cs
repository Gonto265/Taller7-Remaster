// Copyright (c) 2023 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;
using Mediapipe.Tasks.Vision.HandLandmarker;

namespace Mediapipe.Unity
{
    public class HandLandmarkerResultAnnotationController : AnnotationController<MultiHandLandmarkListAnnotation>
    {
        [SerializeField] private bool _visualizeZ = false;

        private readonly object _currentTargetLock = new object();
        private HandLandmarkerResult _currentTarget;

        public void DrawNow(HandLandmarkerResult target)
        {
            target.CloneTo(ref _currentTarget);
            SyncNow();
        }

        public void DrawLater(HandLandmarkerResult target) => UpdateCurrentTarget(target);

        protected void UpdateCurrentTarget(HandLandmarkerResult newTarget)
        {
            lock (_currentTargetLock)
            {
                newTarget.CloneTo(ref _currentTarget);
                isStale = true;
            }
        }

        protected override void SyncNow()
        {
            lock (_currentTargetLock)
            {
                isStale = false;

                annotation.SetHandedness(_currentTarget.handedness);
                annotation.Draw(_currentTarget.handLandmarks, _visualizeZ);

                // =======================================================
                // NUEVO: Enviar Muñeca y Puntas de todos los dedos
                // =======================================================
                if (_currentTarget.handLandmarks != null && _currentTarget.handLandmarks.Count > 0)
                {
                    var hand = _currentTarget.handLandmarks[0];

                    if (hand.landmarks != null && hand.landmarks.Count > 20)
                    {
                        // Extraemos Muñeca(0) y Puntas: Pulgar(4), Índice(8), Medio(12), Anular(16), Meñique(20)
                        float[] datosMano = new float[12];

                        datosMano[0] = hand.landmarks[0].x; datosMano[1] = hand.landmarks[0].y;   // Muñeca
                        datosMano[2] = hand.landmarks[4].x; datosMano[3] = hand.landmarks[4].y;   // Pulgar
                        datosMano[4] = hand.landmarks[8].x; datosMano[5] = hand.landmarks[8].y;   // Índice
                        datosMano[6] = hand.landmarks[12].x; datosMano[7] = hand.landmarks[12].y;  // Medio
                        datosMano[8] = hand.landmarks[16].x; datosMano[9] = hand.landmarks[16].y;  // Anular
                        datosMano[10] = hand.landmarks[20].x; datosMano[11] = hand.landmarks[20].y; // Meñique

                        GameObject managersObj = GameObject.Find("Managers");
                        if (managersObj != null)
                        {
                            managersObj.SendMessage("RecibirManoDesdeMediaPipe", datosMano, SendMessageOptions.DontRequireReceiver);
                        }
                    }
                }
                // =======================================================
            }
        }
    }
}
