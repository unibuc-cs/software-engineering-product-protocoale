import sys
import requests
from bs4 import BeautifulSoup

def get_product_info(query, exact):
    url = f"https://www.carrefour.ro/catalogsearch/result/?q={query}"

    # Send a GET request to the URL and retrieve the HTML content
    response = requests.get(url)

    # Check if the request was successful (status code 200)
    if response.status_code == 200:
        # Parse the HTML content using BeautifulSoup
        soup = BeautifulSoup(response.content, 'html.parser')

        # Find all product containers
        product_containers = soup.find_all('li', class_='product')

        # Extract product information (name and price) for each container
        product_info = []
        for container in product_containers:
            # Extract product id
            product_id = container['data-product-id']

            # Extract product name
            name_element = container.find('div', class_='productItem-name')
            name = name_element.find('a').text.strip() if name_element else 'N/A'
            product_url=container.find('a', class_='productItem-image').get('href')

            # Find the corresponding price element
            price_element = soup.find('div', id=f"product-price-{product_id}")
            price_amount = price_element['data-price-amount'] if price_element else 'N/A'

            # Append product information to the list
            if exact:
                if query.lower() in name.lower().split():
                    product_info.append({'name': name, 'price': price_amount,'product_url':product_url})
            else:
                product_info.append({'name': name, 'price': price_amount,'product_url':product_url})

        return product_info
    else:
        # Handle failed HTTP request
        return None

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
        # print("No products found.")
