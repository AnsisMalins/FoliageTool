﻿using System;
using UnityEngine;

[Serializable]
public struct TextureRule
{
    [Tooltip("Check to disable this rule")]
    public bool bypass;
    
    [Tooltip("The terrain texture to check for")]
    public TerrainLayer layer;
    
    public enum Rule { Include, Exclude }
    [Tooltip("Include or exclude foliage from this texture")]
    public Rule rule;
    
    [Range(0, 1)]
    [Tooltip("The texture threshold at which this rule will be applied")]
    public float threshold;
}