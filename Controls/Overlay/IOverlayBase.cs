using Microsoft.AspNetCore.Components;

namespace FactorioPixelArt.Controls.Overlay;

public interface IOverlayBase : IComponent
{
    public Guid InstanceId { get; }
}
