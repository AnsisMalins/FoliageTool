﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Flora.Core.Brushes;
using Unity.EditorCoroutines.Editor;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Flora.Core
{
    [CustomEditor(typeof(FloraTerrain)), CanEditMultipleObjects]
    public class FloraTerrainEditor : Editor
    {
        private FloraTerrain _fTerrain;
        private void OnEnable()
        {
            _fTerrain = target as FloraTerrain;
            
            List<FloraTerrain> terrains = GetSelectedFloraTerrains();
            RefreshStats(terrains.ToArray());
        }

        private static readonly string[] _dontIncludeMe = new string[]{"m_Script"};

        private readonly GUILayoutOption[] _buttonLayout = new[]
        {
            GUILayout.Height(22)
        };

        List<BiomeBrush> brushes = new List<BiomeBrush>();
        List<BiomeAsset> biomes = new List<BiomeAsset>();
        List<DetailPrototype> detailPrototypes = new List<DetailPrototype>();
        List<TreePrototype> treePrototypes = new List<TreePrototype>();
        int treeInstances = 0;

        void RefreshStats(params FloraTerrain[] terrains)
        {
            brushes.Clear();
            biomes.Clear();
            detailPrototypes.Clear();
            treePrototypes.Clear();
            treeInstances = 0;
                
            foreach (FloraTerrain f in terrains)
            {
                Terrain terrain = f.terrain;
                TerrainData data = terrain.terrainData;
                    
                foreach (var b in BiomeBrush.GetSplines(terrain, true).ToArray())
                {
                    if (brushes.Contains(b))
                        continue;

                    brushes.Add(b);
                }
                    
                foreach (var b in f.GetBiomes())
                {
                    if (biomes.Contains(b))
                        continue;

                    biomes.Add(b);
                }

                foreach (var prototype in data.detailPrototypes)
                {
                    if(detailPrototypes.Contains(prototype))
                        continue;
                        
                    detailPrototypes.Add(prototype);
                }
                    
                foreach (var prototype in data.treePrototypes)
                {
                    if(treePrototypes.Contains(prototype))
                        continue;
                        
                    treePrototypes.Add(prototype);
                }

                treeInstances += data.treeInstanceCount;
            }
        }
        
        private static bool _showStatistics = false;
        public override void OnInspectorGUI()
        {
            List<FloraTerrain> terrains = GetSelectedFloraTerrains();
            
            EditorGUILayout.Space(2);
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Clear", _buttonLayout))
            {
                bool action = EditorUtility.DisplayDialog("Destructive action",
                    "Are you sure you want to remove all foliage on this terrain?", "Yes", "No");
                if (action)
                {
                    foreach (FloraTerrain t in terrains)
                    {
                        if(!t) continue;
                        
                        TerrainRegion region = new TerrainRegion(t.terrain, new Rect(0, 0, 1, 1));
                        t.Clear(region);
                    }
                    
                    RefreshStats(terrains.ToArray());
                }
            }
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Sync", _buttonLayout))
            {
                foreach (FloraTerrain t in terrains)
                {
                    if(!t) continue;
                    
                    t.SyncFoliage(out DetailPrototype[] prototypes);
                }
                
                RefreshStats(terrains.ToArray());
            }

            if (GUILayout.Button("Refresh", _buttonLayout))
            {
                EditorCoroutineUtility.StartCoroutine(Refresh(terrains.ToArray()), this);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
            
            DrawPropertiesExcluding(serializedObject, _dontIncludeMe);

            if (serializedObject.ApplyModifiedProperties())
            {
                RefreshStats(terrains.ToArray());
            }

            EditorGUILayout.Space(5);

            _showStatistics = EditorGUILayout.BeginFoldoutHeaderGroup(_showStatistics, "Statistics");
            if (_showStatistics)
            {
                string splineCountString = $"Splines: {brushes.Count}";
                string biomeCountString = $"Biomes: {biomes.Count}";
                string detailCountString = $"Detail Prototypes: {detailPrototypes.Count}";
                string treePrototypesString = $"Tree Prototypes: {treePrototypes.Count}";
                string treeCountString = $"Tree Instances: {treeInstances}";
                
                GUILayout.Label(splineCountString);
                GUILayout.Label(biomeCountString);
                GUILayout.Label(detailCountString);
                GUILayout.Label(treePrototypesString);
                GUILayout.Label(treeCountString);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        IEnumerator Refresh(params FloraTerrain[] terrains)
        {
            foreach (FloraTerrain t in terrains)
            {
                yield return null;
                if(!t)
                    continue;

                yield return EditorCoroutineUtility.StartCoroutine(FloraTerrain.Refresh(t), this);
            }
        }

        List<FloraTerrain> GetSelectedFloraTerrains()
        {
            List<FloraTerrain> list = new List<FloraTerrain>();
            foreach (Object o in targets)
            {
                if(!o) continue;
                    
                FloraTerrain t = o as FloraTerrain;
                if (t)
                {
                    list.Add(t);
                } 
            }

            return list;
        }
    }
}