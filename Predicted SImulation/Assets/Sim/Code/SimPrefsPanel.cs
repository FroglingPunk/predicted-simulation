using TMPro;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class SimPrefsPanel : MonoBehaviour
{
    [SerializeField] private Slider _sliderFieldSize;
    [SerializeField] private Slider _sliderUnitsCount;
    [SerializeField] private Slider _sliderSimulationSpeed;

    [SerializeField] private TextMeshProUGUI _textUnitsCountMax;


    private void Start()
    {
        var maxUnitsCount = SimPrefs.FieldSize.Value * SimPrefs.FieldSize.Value / 4;
        _textUnitsCountMax.text = maxUnitsCount.ToString();
        _sliderUnitsCount.maxValue = maxUnitsCount;

        _sliderFieldSize.SetValueWithoutNotify(SimPrefs.FieldSize.Value);
        _sliderUnitsCount.SetValueWithoutNotify(SimPrefs.UnitsCount.Value);
        _sliderSimulationSpeed.SetValueWithoutNotify(SimPrefs.SimulationSpeed.Value);


        _sliderUnitsCount.onValueChanged.AddListener((value) => SimPrefs.UnitsCount.Value = (int)value);
        _sliderSimulationSpeed.onValueChanged.AddListener((value) => SimPrefs.SimulationSpeed.Value = value);

        _sliderFieldSize.OnEndDragAsObservable().Subscribe(_ =>
        {
            var sliderValue = (int)_sliderFieldSize.value;

            if (sliderValue == SimPrefs.FieldSize.Value)
            {
                return;
            }

            var maxUnitsCount = sliderValue * sliderValue / 2;
            _textUnitsCountMax.text = maxUnitsCount.ToString();
            _sliderUnitsCount.maxValue = maxUnitsCount;

            SimPrefs.FieldSize.Value = sliderValue;
        });
    }
}