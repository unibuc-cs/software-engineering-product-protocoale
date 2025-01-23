import sys
import requests
from bs4 import BeautifulSoup

def get_product_info(query, exact):
    url = f"https://www.auchan.ro/{query}"

    # Send a GET request to the URL and retrieve the HTML content
    response = requests.get(url)

    # Check if the request was successful (status code 200)
    if response.status_code == 200:
        # Parse the HTML content using BeautifulSoup
        soup = BeautifulSoup(response.content, 'html.parser')

        # Find all product containers
        products = soup.find_all('section', class_='vtex-product-summary-2-x-container vtex-product-summary-2-x-container--defaultShelf vtex-product-summary-2-x-container--searchShelf vtex-product-summary-2-x-containerNormal vtex-product-summary-2-x-containerNormal--defaultShelf vtex-product-summary-2-x-containerNormal--searchShelf overflow-hidden br3 h-100 w-100 flex flex-column justify-between center tc')

        # Extract product information (name and price) for each container
        product_info = []
        for product in products:
            # Extract product id
            
            product_url=f"https://www.auchan.ro/{product.find('a', class_='vtex-product-summary-2-x-clearLink vtex-product-summary-2-x-clearLink--defaultShelf vtex-product-summary-2-x-clearLink--searchShelf h-100 flex flex-column').get('href')}"
            name=product.find('span', class_='vtex-product-summary-2-x-productBrand vtex-product-summary-2-x-productBrand--defaultShelf vtex-product-summary-2-x-brandName vtex-product-summary-2-x-brandName--defaultShelf t-body').get_text()
            
            price_element=product.find('div', class_='auchan-store-theme-4-x-pricePlpContainer')
            integer=price_element.find('span', class_='vtex-product-price-1-x-currencyInteger vtex-product-price-1-x-currencyInteger--shelfPrice').get_text()
            fraction=price_element.find('span', class_='vtex-product-price-1-x-currencyFraction vtex-product-price-1-x-currencyFraction--shelfPrice').get_text()

            price_amount=integer+','+fraction
                       
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
    else:
        print("No products found.")
