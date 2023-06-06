using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsCastDebugger
{
    [System.Serializable]
    public class GizmosSettings
    {
        public bool drawSelected = true;
        public bool drawOrigin = true;
        public bool drawHitPoint = true;
        public bool drawNewCast = true;
        public bool usefadeEffect = true;
        public float gizmosLifeTime = 5f;
        public float pointSize = 0.5f;
        public Color castColor = Color.white;
        public Color originPointColor = Color.white;
        public Color hitPointColor = Color.yellow;
    }
}
