using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Jobs;

using Object = UnityEngine.Object;
using Random = Unity.Mathematics.Random;

namespace Sketch {

public class InstancePool : IDisposable
{
    #region Public properties

    public int Capacity
      { get => _instances.Count; set => ChangeCapacity(value); }

    public ReadOnlySpan<Mesh> Meshes
      { get => _meshes; set => ResetMeshes(value); }

    public ReadOnlySpan<Material> Materials
      { get => _materials; set => ResetMaterials(value); }

    public uint RandomSeed
      { get => _randomSeed; set => ResetRandomSeed(value); }

    public int Layer
      { get => _layer; set => ResetLayer(value); }

    public TransformAccessArray Xforms => UpdateXforms();

    #endregion

    #region Public methods

    public InstancePool()
      => _mpblock = new MaterialPropertyBlock();

    public void Dispose()
      => ChangeCapacity(0);

    #endregion

    #region Private members

    static readonly Type[] InstanceComponents =
      { typeof(MeshFilter), typeof(MeshRenderer) };

    List<GameObject> _instances = new List<GameObject>();

    Mesh[] _meshes = new Mesh[] { null };
    Material[] _materials = new Material[] { null };
    uint _randomSeed = 1;
    int _layer;

    TransformAccessArray _xforms;
    MaterialPropertyBlock _mpblock;

    #endregion

    #region Allocation / deallocation

    void AddNewInstance()
    {
        var i = _instances.Count;

        var go = new GameObject("Instance", InstanceComponents);
        go.hideFlags = HideFlags.HideAndDontSave;
        go.layer = _layer;

        go.GetComponent<MeshFilter>().sharedMesh = GetMeshForIndex(i);

        var rend = go.GetComponent<MeshRenderer>();
        rend.sharedMaterial = GetMaterialForIndex(i);
        rend.SetPropertyBlock(_mpblock);

        _instances.Add(go);
        InvalidateXforms();
    }

    void RemoveLastInstance()
    {
        var i = _instances.Count - 1;

        if (Application.isPlaying)
            Object.Destroy(_instances[i]);
        else
            Object.DestroyImmediate(_instances[i]);

        _instances.RemoveAt(i);
        InvalidateXforms();
    }

    void ChangeCapacity(int capacity)
    {
        capacity = Mathf.Clamp(capacity, 0, 0x20000);
        while (_instances.Count < capacity) AddNewInstance();
        while (_instances.Count > capacity) RemoveLastInstance();
    }

    #endregion

    #region Transform access array

    void InvalidateXforms()
    {
        if (_xforms.isCreated) _xforms.Dispose();
    }

    TransformAccessArray UpdateXforms()
    {
        if (!_xforms.isCreated)
            _xforms = new TransformAccessArray
              (_instances.Select(go => go.transform).ToArray());
        return _xforms;
    }

    #endregion

    #region Mesh / material methods

    bool CompareMeshes(ReadOnlySpan<Mesh> meshes)
    {
        if (_meshes.Length != meshes.Length) return false;
        for (var i = 0; i < _meshes.Length; i++)
            if (_meshes[i] != meshes[i]) return false;
        return true;
    }

    bool CompareMaterials(ReadOnlySpan<Material> materials)
    {
        if (_materials.Length != materials.Length) return false;
        for (var i = 0; i < _materials.Length; i++)
            if (_materials[i] != materials[i]) return false;
        return true;
    }

    Mesh GetMeshForIndex(int i)
    {
        var rand = Random.CreateFromIndex(RandomSeed ^ 0xcbd ^ (uint)i);
        return _meshes[rand.NextInt(_meshes.Length)];
    }

    Material GetMaterialForIndex(int i)
    {
        var rand = Random.CreateFromIndex(RandomSeed ^ 0x5a3 ^ (uint)i);
        return _materials[rand.NextInt(_materials.Length)];
    }

    void ResetMeshes(ReadOnlySpan<Mesh> meshes)
    {
        if (CompareMeshes(meshes)) return;
        _meshes = meshes.ToArray();
        ResetRandomSeed();
    }

    void ResetMaterials(ReadOnlySpan<Material> materials)
    {
        if (CompareMaterials(materials)) return;
        _materials = materials.ToArray();
        ResetRandomSeed();
    }

    void ResetRandomSeed(uint? seed = null)
    {
        if (_randomSeed == seed) return;
        if (seed != null) _randomSeed = (uint)seed;
        for (var i = 0; i < _instances.Count; i++)
        {
            var t = _instances[i];
            t.GetComponent<MeshFilter>().sharedMesh = GetMeshForIndex(i);
            t.GetComponent<MeshRenderer>().sharedMaterial = GetMaterialForIndex(i);
        }
    }

    void ResetLayer(int layer)
    {
        if (_layer == layer) return;
        _layer = layer;
        foreach (var go in _instances) go.layer = _layer;
    }

    #endregion
}

} // namespace Sketch
