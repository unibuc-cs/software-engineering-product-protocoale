# Auto Shopper for grocery stores

## The following stores are supported:

- Carrefour
- Mega Image
- Auchan

## Dependencies:

1. Install Google Chrome
2. Install Python
3. Install PIP
4. Install the following packets:

```
pip install selenium
pip install webdriver-manager
```

## Usage:

```
python3 shopper.py "{product_link_1}" "{product_link_2}" ... "{product_link_N}" 
```

### Example

```
python3 shopper.py "https://carrefour.ro/produse/pulover-tex-dama-craciun-xs-xxl-69-63515656" "https://carrefour.ro/produse/pijama-tex-copii-craciun-3-14-ani-69-62520433" "https://www.mega-image.ro/Paine-cafea-cereale-si-mic-dejun/Paine-si-specialitati/Cozonac-si-chec/Cozonac-Bucovinean-900g/p/21960" "https://www.auchan.ro/nuci-in-coaja-filiera-auchan-1-kg/p"
```

## In order to buy more items of the same product, just paste the link for desired ammount of times.

### Example
```
python3 shopper.py "https://carrefour.ro/produse/pulover-tex-dama-craciun-xs-xxl-69-63515656" "https://carrefour.ro/produse/pulover-tex-dama-craciun-xs-xxl-69-63515656"

```

# Warning: Do NOT close the login window, you will break the program! The window closes automatically.
