using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] private float _speed = 1f;

    private void Update()
    {
        transform.rotation *= Quaternion.Euler(0f, _speed * Time.deltaTime, 0f);   

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}