using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset; 

    void LateUpdate()
    {
        if(player == null) return;
        
        float targetX = player.position.x + offset.x;
        
        transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
    }
}