using UniRx;
using UnityEngine;

public class CameraPositionSetter : MonoBehaviour
{
    private void Start()
    {
        transform.position = new Vector3(SimPrefs.FieldSize.Value * 0.5f, SimPrefs.FieldSize.Value, SimPrefs.FieldSize.Value * 0.5f);

        SimPrefs.FieldSize.Subscribe(fieldSize =>
        {
            transform.position = new Vector3(fieldSize * 0.5f, SimPrefs.FieldSize.Value, fieldSize * 0.5f);
        });
    }
}