using UnityEngine;

namespace App
{
    public interface IEntity
    {
        Vector3 Position { get; }

        void SetPosition(Vector3 position);
        void AddForce(Vector3 position);
    }

}