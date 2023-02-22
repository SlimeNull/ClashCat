using YamlDotNet.Serialization;

namespace ClashCat.Models.Clash.Configuration
{
    public enum ClashProxyType
    {
        [YamlMember(Alias = "ss")]
        ShadowSocks
    }
}