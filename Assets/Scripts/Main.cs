using System.Collections;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 618, 649
public class Main : MonoBehaviour
{
    //поля ввода
    [SerializeField] private InputField _inputAreaWidth;
    [SerializeField] private InputField _inputAreaHeight;
    [SerializeField] private InputField _inputTileWidth;
    [SerializeField] private InputField _inputTileHeight;
    [SerializeField] private InputField _inputSeamRows;
    [SerializeField] private InputField _inputSeamInRow;
    [SerializeField] private InputField _inputRowsOffset;
    [SerializeField] private InputField _inputAngle;

    //текущие значения полей ввода
    private float _areaWidth;
    private float _areaHeight;
    private float _tileWidth;
    private float _tileHeight;
    private float _seamRows;
    private float _seamInRow;
    private float _rowsOffset;
    private float _angle;

    //поля вывода
    [SerializeField] private Text _textUpdating;
    [SerializeField] private Text _textSquare;
    [SerializeField] private Text _textError;

    [SerializeField] private RectTransform _area; //видимая рабочая область (далее - рабочая область)
    [SerializeField] private RectTransform _areaFill; //квадратная область, полностью заполняющаяся плитками (далее - квадрат)
    [SerializeField] private GameObject _tilePrefab; //заготовка плитки

    void Start()
    {
        OnInputValueChanged();
    }

    //очистить рабочую область
    private void ClearArea()
    {
        while (_areaFill.transform.childCount > 0)
            DestroyImmediate(_areaFill.transform.GetChild(0).gameObject);
    }

    //вводимое значение изменено
    public void OnInputValueChanged()
    {
        //скрыть предыдущее возможное сообщение об ошибке
        _textError.text = "";

        //считать входные данные и проверить их на корректность
        if (!float.TryParse(_inputAreaWidth.text, out _areaWidth)) { ShowError("Area width is incorrect!"); return; }
        if (!float.TryParse(_inputAreaHeight.text, out _areaHeight)) { ShowError("Area height is incorrect!"); return; }
        if (!float.TryParse(_inputTileWidth.text, out _tileWidth)) { ShowError("Tile width is incorrect!"); return; }
        if (!float.TryParse(_inputTileHeight.text, out _tileHeight)) { ShowError("Tile height is incorrect!"); return; }
        if (!float.TryParse(_inputSeamRows.text, out _seamRows)) { ShowError("Seam between rows is incorrect!"); return; }
        if (!float.TryParse(_inputSeamInRow.text, out _seamInRow)) { ShowError("Seam between tiles in a row is incorrect!"); return; }
        if (!float.TryParse(_inputRowsOffset.text, out _rowsOffset)) { ShowError("Rows offset is incorrect!"); return; }
        if (!float.TryParse(_inputAngle.text, out _angle)) { ShowError("Angle of rotation is incorrect!"); return; }

        //проверить размеры на отрицательные значения
        if (_areaWidth <= 0 || _areaHeight <= 0 || _tileWidth <= 0 || _tileHeight <= 0)
        {
            ShowError("All sizes must be positive!");
            return;
        }

        //показать текст обновления
        _textUpdating.enabled = true;

        //запустить процесс обновления
        StartCoroutine(UpdateView());
    }

    //сообщить об ошибке
    private void ShowError(string text)
    {
        _textError.text = text;
    }

    //обновить раскладку плитки
    private IEnumerator UpdateView()
    {
        yield return new WaitForSeconds(0.01f);

        //очистить старую раскладку
        ClearArea();
        _textSquare.text = "";

        //применить размеры к рабочей области
        _area.sizeDelta = new Vector2(_areaWidth, _areaHeight);

        //повернуть квадрат
        _areaFill.rotation = Quaternion.Euler(0, 0, _angle);

        /*
        Плитки заполняют весь квадрат, внешний по отношению к рабочей области.
        Сторона этого квадрата вдвое больше, чем большая сторона рабочей области.
        Таким образом, вся рабочая область будет гарантированно заполнена при любом повороте.
        */

        //определить размер квадрата для заполнения плитками
        float maxAreaSide = Mathf.Max(_areaWidth, _areaHeight); //большая сторона рабочей области
        float areaSetSide = maxAreaSide * 2; //сторона внешнего квадрата, заполняемого плитками
        _areaFill.sizeDelta = new Vector2(areaSetSide, areaSetSide); //применить

        //определить Y нижней и X левой границы рабочей области по отношению к квадрату
        float areaYMin = (areaSetSide - _areaHeight) / 2; //Y нижней границы
        float areaXMin = (areaSetSide - _areaWidth) / 2; //X левой границы

        float periodY = _tileHeight + _seamRows; //период вертикального заполнения
        float periodX = _tileWidth + _seamInRow; //период горизонтального заполнения

        //заполнить квадрат плитками
        float totalSquare = 0; //суммарная площадь видимых частей плиток
        float firstRowY = areaYMin - (Mathf.Floor(areaYMin / periodY) + 1) * periodY; //координата Y крайнего нижнего ряда
        float lastRowY = areaYMin + (Mathf.Floor((areaSetSide - areaYMin) / periodY) + 1) * periodY; //координата Y крайнего верхнего ряда
        for (float y = firstRowY; y <= lastRowY; y += periodY) //для каждого ряда...
        {
            float currentOffset = _rowsOffset * Mathf.Floor((y - areaYMin) / periodY) % periodX; //смещение текущего ряда относительного нижнего в рабочей области
            float firstX = areaXMin - (Mathf.Floor(areaXMin / periodX) + 1) * periodX + currentOffset; //координата X крайней левой плитки
            float lastX = areaXMin + (Mathf.Floor((areaSetSide - areaXMin) / periodX) + 1) * periodX + currentOffset; //координата X крайней правой плитки
            for (float x = firstX; x <= lastX; x += periodX) //для каждой плитки...
            {
                GameObject tile = Instantiate(_tilePrefab, _areaFill.transform); //поставить плитку на квадрат
                RectTransform rect = tile.GetComponent<RectTransform>(); //получить её RectTransform
                rect.anchoredPosition = new Vector2(x, y); //задать позицию на квадрате
                rect.sizeDelta = new Vector2(_tileWidth, _tileHeight); //применить размеры

                //получить массивы координат углов рабочей области и углов плитки, и преобразовать из Vector3 в Vector2
                Vector3[] areaCorners3 = new Vector3[4];
                Vector2[] areaCorners2 = new Vector2[4];
                Vector3[] tileCorners3 = new Vector3[4];
                Vector2[] tileCorners2 = new Vector2[4];
                _area.GetWorldCorners(areaCorners3);
                rect.GetWorldCorners(tileCorners3);
                for (int i = 0; i < 4; i++)
                {
                    areaCorners2[i] = areaCorners3[i];
                    tileCorners2[i] = tileCorners3[i];
                }

                //вычислить площадь видимой части плитки
                float square = GeometryUtils.GetIntersectSquare(areaCorners2, tileCorners2);

                //добавить эту площадь к общей площади
                totalSquare += square;
            }
        }

        //вывести площадь на экран
        _textSquare.text = "Square: " + totalSquare;

        //скрыть текст обновления
        _textUpdating.enabled = false;
    }
}
