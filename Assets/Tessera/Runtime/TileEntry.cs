﻿using System;

namespace Tessera
{
    /// <summary>
    /// Specifies a tile to be used by <see cref="TesseraGenerator"/>
    /// </summary>
    [Serializable]
    public class TileEntry
    {
        /// <summary>
        /// The tile to use
        /// </summary>
        public TesseraTileBase tile;

        /// <summary>
        /// The weight controls the relative probability of this tile being selected.
        /// I.e. tile wiht weight of 2.0 is twice common in the generation than a tile with weight 1.0.
        /// </summary>
        public float weight = 1.0f;
    }
}