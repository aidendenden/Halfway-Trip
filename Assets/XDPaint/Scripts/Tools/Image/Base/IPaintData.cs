using System.Collections;
using UnityEngine;
using XDPaint.Core;
using XDPaint.Core.Layers;
using XDPaint.Core.Materials;
using XDPaint.Core.PaintModes;
using XDPaint.States;
using XDPaint.Utils;

namespace XDPaint.Tools.Image.Base
{
    public interface IPaintData : IDisposable
    {
        IStatesController StatesController { get; }
        ILayersController LayersController { get; }
        IRenderTextureHelper TexturesHelper { get; }
        IRenderComponentsHelper RenderComponents { get; }
        IBrush Brush { get; }
        IPaintMode PaintMode { get; }
        Camera Camera { get; }
        Material Material { get; }
        CommandBufferBuilder CommandBuilder { get; }
        Mesh QuadMesh { get; }
        bool IsPainted { get; }
        bool IsPainting { get; }
        bool InBounds { get; }

        void Render();
        void SaveState();
        void StartCoroutine(IEnumerator coroutine);
        void StopCoroutine(IEnumerator coroutine);
    }
}