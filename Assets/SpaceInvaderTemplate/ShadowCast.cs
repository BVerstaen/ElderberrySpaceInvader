using System;
using UnityEngine;

public class ShadowCast : MonoBehaviour
{
    [SerializeField] private Vector3 _localPosition;
    [SerializeField] private Color _shadowColor;
    
    private SpriteRenderer _shadowSprite = null;
    private SpriteRenderer _spriteRenderer => GetComponent<SpriteRenderer>();

    private void Start()
    {
        GameObject shadowObject = Instantiate(new GameObject(), transform);
        shadowObject.name = $"{transform.name}_Shadow";
        shadowObject.transform.localPosition = _localPosition;

        _shadowSprite = shadowObject.AddComponent<SpriteRenderer>();
        _shadowSprite.material = _spriteRenderer.material;
        _shadowSprite.color = _shadowColor;

        if (GameFeelManager.Instance != null)
        {
            _shadowSprite.gameObject.SetActive(GameFeelManager.Instance.IsFeatureActive("EnnemyBlobAnimation"));
            GameFeelManager.Instance.OnFeatureToggled += OnFeatureToggled;
        }
    }

    private void OnDestroy()
    {
        if (GameFeelManager.Instance != null) GameFeelManager.Instance.OnFeatureToggled -= OnFeatureToggled;
    }

    private void OnFeatureToggled(string feature, bool IsActive)
    {
        if (feature == "EnnemyBlobAnimation") _shadowSprite.gameObject.SetActive(IsActive);
    }

    private void Update()
    {
        if(_shadowSprite != null)
            _shadowSprite.sprite = _spriteRenderer.sprite;
    }
}
