import sys
import requests
from bs4 import BeautifulSoup

def get_product_info(query):
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

            # Find the corresponding price element
            price_element = soup.find('div', id=f"product-price-{product_id}")
            price_amount = price_element['data-price-amount'] if price_element else 'N/A'

            # Append product information to the list
            if query.lower() in name.lower().split(): #daca vreau sa verific exact sa apara paine
                product_info.append({'name': name, 'price': price_amount})

        return product_info
    else:
        # Handle failed HTTP request
        return None

if __name__ == "__main__":
    #print("INCEPE PYTHONULL")
    if len(sys.argv) != 2:
        print("Usage: python_script.py <query>")
        sys.exit(1)

    query = sys.argv[1]
    products = get_product_info(query)
    if products:
        for product in products:
            print(f"Product: {product['name']}, Price: {product['price']} Lei")
    else:
        print("No products found.")
