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

                // El código original de MediaPipe
                annotation.SetHandedness(_currentTarget.handedness);
                annotation.Draw(_currentTarget.handLandmarks, _visualizeZ);

                // =======================================================
                // NUEVO: Enviar datos de la mano a nuestro juego
                // =======================================================
                if (_currentTarget.handLandmarks != null && _currentTarget.handLandmarks.Count > 0)
                {
                    var hand = _currentTarget.handLandmarks[0]; // Tomamos la primera mano detectada

                    // Quitamos el "hand != null" porque NormalizedLandmarks es un struct
                    if (hand.landmarks != null && hand.landmarks.Count > 8)
                    {
                        // Extraemos X e Y de la punta del índice (Landmark 8) y pulgar (Landmark 4)
                        Vector2 indexTip = new Vector2(hand.landmarks[8].x, hand.landmarks[8].y);
                        Vector2 thumbTip = new Vector2(hand.landmarks[4].x, hand.landmarks[4].y);

                        // Buscamos nuestro objeto Managers en la escena
                        GameObject managersObj = GameObject.Find("Managers");
                        if (managersObj != null)
                        {
                            // Empaquetamos los 4 valores y enviamos el mensaje
                            Vector4 datosMano = new Vector4(indexTip.x, indexTip.y, thumbTip.x, thumbTip.y);
                            managersObj.SendMessage("RecibirManoDesdeMediaPipe", datosMano, SendMessageOptions.DontRequireReceiver);
                        }
                    }
                }
                // =======================================================
            }
        }
    }
}
