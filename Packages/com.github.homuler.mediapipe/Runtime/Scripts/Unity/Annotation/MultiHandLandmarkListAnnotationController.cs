// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections.Generic;
using UnityEngine;

namespace Mediapipe.Unity
{
    public class MultiHandLandmarkListAnnotationController : AnnotationController<MultiHandLandmarkListAnnotation>
    {
        [SerializeField] private bool _visualizeZ = false;

        private IReadOnlyList<NormalizedLandmarkList> _currentHandLandmarkLists;
        private IReadOnlyList<ClassificationList> _currentHandedness;

        public void DrawNow(IReadOnlyList<NormalizedLandmarkList> handLandmarkLists, IReadOnlyList<ClassificationList> handedness = null)
        {
            _currentHandLandmarkLists = handLandmarkLists;
            _currentHandedness = handedness;
            SyncNow();
        }

        public void DrawLater(IReadOnlyList<NormalizedLandmarkList> handLandmarkLists)
        {
            UpdateCurrentTarget(handLandmarkLists, ref _currentHandLandmarkLists);
        }

        public void DrawLater(IReadOnlyList<ClassificationList> handedness)
        {
            UpdateCurrentTarget(handedness, ref _currentHandedness);
        }

        protected override void SyncNow()
        {
            isStale = false;
            annotation.Draw(_currentHandLandmarkLists, _visualizeZ);

            if (_currentHandedness != null)
            {
                annotation.SetHandedness(_currentHandedness);
            }
            _currentHandedness = null;

            // =======================================================
            // NUEVO: Enviar datos de la mano a nuestro juego (Corregido)
            // =======================================================
            if (_currentHandLandmarkLists != null && _currentHandLandmarkLists.Count > 0)
            {
                var hand = _currentHandLandmarkLists[0]; // Tomamos la primera mano detectada

                if (hand.Landmark != null && hand.Landmark.Count > 8)
                {
                    // Extraemos X e Y de la punta del índice (Landmark 8) y pulgar (Landmark 4)
                    Vector2 indexTip = new Vector2(hand.Landmark[8].X, hand.Landmark[8].Y);
                    Vector2 thumbTip = new Vector2(hand.Landmark[4].X, hand.Landmark[4].Y);

                    // Buscamos el objeto Managers en la escena
                    GameObject managersObj = GameObject.Find("Managers");
                    if (managersObj != null)
                    {
                        // Empaquetamos los 4 valores en un Vector4 para que puedan viajar juntos
                        Vector4 datosMano = new Vector4(indexTip.x, indexTip.y, thumbTip.x, thumbTip.y);

                        // Enviamos el mensaje sin importar las restricciones de Assembly
                        managersObj.SendMessage("RecibirManoDesdeMediaPipe", datosMano, SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
            // =======================================================
        }
    }
}
