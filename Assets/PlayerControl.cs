using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerControl : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (!IsOwner)
            return;

        var pos = transform.position;
        pos.x += Random.Range(-5, 5);
        transform.position = pos;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
            return;

        var pos = transform.position;

        if (Input.GetKeyDown(KeyCode.A))
            pos.x -= Time.deltaTime * 50;
        if (Input.GetKeyDown(KeyCode.D))
            pos.x += Time.deltaTime * 50;
        if (Input.GetKeyDown(KeyCode.W))
            pos.z += Time.deltaTime * 50;
        if (Input.GetKeyDown(KeyCode.S))
            pos.z -= Time.deltaTime * 50;

        transform.position = pos;
    }
}
