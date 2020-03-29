using System;
using UnityEngine;

namespace CarnationVariableSectionPart.UI
{
    public struct TextureDefinition : IEquatable<TextureDefinition>
    {
        public string name;
        public Texture2D diff;
        public Texture2D norm;
        public Texture2D spec;
        public string directory;
        public string diffuse;
        public string normals;
        public string specular;
        public string shader;
        public bool Equals(TextureDefinition other) => other.name != null && name.Equals(other.name);
        public static bool operator ==(TextureDefinition lhs, TextureDefinition rhs) => lhs.Equals(rhs);
        public static bool operator !=(TextureDefinition lhs, TextureDefinition rhs) => lhs.Equals(rhs);
    }
}
