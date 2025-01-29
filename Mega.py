import sys
import time
from selenium import webdriver
from selenium.webdriver.chrome.service import Service as ChromeService
from webdriver_manager.chrome import ChromeDriverManager
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.common.by import By
import requests

# Check if the query is provided as a command-line argument
def get_product_info(query, exact):
    url = f"https://www.mega-image.ro/search?q={query}%3Arelevance&sort=relevance"

    # Send a GET request to the URL and retrieve the HTML content
    options=Options()
    options.add_argument('--headless')
    driver = webdriver.Chrome(service=ChromeService(ChromeDriverManager().install()),options=options)

    response = requests.get(url)
    driver.get(url)

    # Check if the request was successful (status code 200)
    if response.status_code == 200:
        time.sleep(3)
        button= driver.find_element(By.CSS_SELECTOR, "button[data-testid='cookie-popup-accept']")
        button.click()
        # Find all product products

        products_name = [product.text for product in driver.find_elements(By.CSS_SELECTOR, "span[data-testid='product-name']" )]
        products_price=[product.find_element(By.CSS_SELECTOR,"div[class='sc-dqia0p-9 jWCjCP']").text+','+product.find_element(By.CSS_SELECTOR,"sup[class='sc-dqia0p-10 cNzomO']").text
                        for product in driver.find_elements(By.CSS_SELECTOR, "div[data-testid='product-block-price']")]
        products_link=[product.get_attribute('href') for product in driver.find_elements(By.CSS_SELECTOR, "a[data-testid='product-block-name-link']" )]
        products=zip(products_name,products_price, products_link)

        # Extract product information (name and price) for each product
        product_info = []
        for name, price_amount, product_url  in products:

            # Append product information to the list
            if len(name) > 0:
                if exact:
                    if query.lower() in name.lower().split():
                        product_info.append({'name': name, 'price': price_amount,'product_url':product_url})
                else:
                    product_info.append({'name': name, 'price': price_amount,'product_url':product_url})

        return (None if product_info == [] else product_info)
    else:
        # Handle failed HTTP request
        return None

# Print the text inside each article element along with its price
if __name__ == "__main__":
    if len(sys.argv) < 2 or len(sys.argv) > 3:
        print("Usage: python_script.py <query> [exact]")
        sys.exit(1)

    query = sys.argv[1]
    exact = False
    if len(sys.argv) == 3:
        exact = True

    products = get_product_info(query, exact)
    if products:
        for product in products:
            print(f"{product['name']}\n{product['price']}\n{product['product_url']}")
            print("-" * 50)
        # else:
        #     print("No products found.")
