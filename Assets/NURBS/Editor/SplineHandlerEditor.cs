﻿using UnityEngine;
using UnityEditor;
using System.Linq;

namespace kmty.NURBS {
    [CustomEditor(typeof(SplineHandler))]
    public class SplineHandlerEditor : Editor {
        Spline spline;
        int selectedId = -1;
        int length;

        void OnEnable() {
            var hdl = (SplineHandler)target;
            Init(hdl.Data);
        }

        void OnSceneGUI() {
            var hdl = (SplineHandler)target;
            var cps = hdl.Data.cps;
            if (cps.Count != length) Init(hdl.Data);

            for (int i = 0; i < length; i++) {
                var c = hdl.Data.cps[i];
                spline.SetCP(i, new CP(hdl.transform.position + c.pos, c.weight));
            }

            for(var i = 0; i < cps.Count; i++) {
                var cp = cps[i];
                var wp = hdl.transform.TransformPoint(cp.pos);
                var sz = HandleUtility.GetHandleSize(wp) * 0.1f;
                if (Handles.Button(wp, Quaternion.identity, sz, sz, Handles.CubeHandleCap)) {
                    selectedId = i;
                    Repaint();
                }
            }

            if (selectedId > -1) {
                var cp = cps[selectedId];
                var wp = hdl.transform.TransformPoint(cp.pos);
                EditorGUI.BeginChangeCheck();
                var p = Handles.DoPositionHandle(wp, Quaternion.identity);
                if (EditorGUI.EndChangeCheck()) {
                    cp.pos = hdl.transform.InverseTransformPoint(p);
                    cps[selectedId] = cp;
                    EditorUtility.SetDirty(hdl.Data);
                }
            }

            Draw(spline, hdl);
        }

        void Init(SplineCpsData data) {
            var hdl = (SplineHandler)target;
            var trs = hdl.transform;
            var cps = data.cps.Select(c => new CP(trs.TransformPoint(c.pos), c.weight)).ToArray();
            length = cps.Length;
            spline = new Spline(cps, data.order, data.type);
        }

        void Draw(Spline s, SplineHandler h) {
            var seg = 0.005f;
            if (h.showSegments) {
                Handles.color = Color.grey;
                for (int i = 0; i < s.cps.Length - 1; i++)
                    Handles.DrawLine(s.cps[i].pos, s.cps[i + 1].pos);
            }
            for (float t = 0; t < 1 - seg; t += seg) {
                s.GetCurve(t, out Vector3 va);
                s.GetCurve(t + seg, out Vector3 vb);
                s.GetFirstDerivative(t, out Vector3 d1);
                s.GetSecondDerivative(t, out Vector3 d2);
                var c1 = d1 * 0.1f + Vector3.one * 0.5f;
                var c2 = d2 * 0.01f + Vector3.one * 0.5f;
                if (h.show1stDerivative) {
                    Handles.color = new Vector4(c1.x, c1.y, c1.z, 1);
                    Handles.DrawLine(va, va + d1 * 0.05f);
                 }
                if (h.show2ndDerivative) {
                    Handles.color = new Vector4(c2.x, c2.y, c2.z, 1);
                    Handles.DrawLine(va, va + d2 * 0.001f);
                 }
                Handles.color = Color.cyan;
                Handles.DrawLine(va, vb);
            }
        }
    }
}
