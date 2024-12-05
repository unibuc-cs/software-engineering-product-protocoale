import sys
import requests
from bs4 import BeautifulSoup
from unidecode import unidecode  # Import the unidecode module

def scrape_kaufland(query):
    # URL of the page to scrape
    url = f"https://www.kaufland.ro/cautarii.html?q={query}"

    try:
        # Send a GET request to the URL
        response = requests.get(url)

        # Check if the request was successful
        if response.status_code == 200:
            # Parse the HTML content
            soup = BeautifulSoup(response.content, "html.parser")

            # Find all divs containing product information
            product_divs = soup.find_all("div", class_="g-col t-search-result__list-item")
            
            if len(product_divs) == 0:
                print("Product Name:", "none")
                print("Product Subtitle:", "none")
                print("Product Price:", "none")
                print("Product Quantity:", "none")
                print("-" * 50)
                return

            # Iterate through each product div and extract information
            for div in product_divs:
                # Extract product name
                product_name_tag = div.find("h4", class_="m-offer-tile__title")
                product_name = product_name_tag.text.strip() if product_name_tag else "N/A"

                # Extract product subtitle
                product_subtitle_tag = div.find("h5", class_="m-offer-tile__subtitle")
                product_subtitle = product_subtitle_tag.text.strip() if product_subtitle_tag else "N/A"

                # Extract product price
                product_price_tag = div.find("div", class_="a-pricetag__price")
                product_price = product_price_tag.text.strip() if product_price_tag else "N/A"

                # Extract product quantity
                product_quantity_tag = div.find("div", class_="m-offer-tile__quantity")
                product_quantity = product_quantity_tag.text.strip() if product_quantity_tag else "N/A"

                # Normalize text to ASCII and replace diacritics with normal letters
                product_name = unidecode(product_name)
                product_subtitle = unidecode(product_subtitle)
                product_price = unidecode(product_price)
                product_quantity = unidecode(product_quantity)

                # Print product information
                if query.lower() in product_name.lower() or query.lower() in product_subtitle.lower():
                    print("Product Name:", product_name)
                    print("Product Subtitle:", product_subtitle)
                    print("Product Price:", product_price)
                    print("Product Quantity:", product_quantity)
                    print("-" * 50)

        else:
            print(f"Failed to retrieve the web page: {response.status_code}")

    except Exception as e:
        print(f"An error occurred: {e}")

# Extract the query from command line arguments
if len(sys.argv) > 1:
    query = " ".join(sys.argv[1:])
    scrape_kaufland(query)
else:
    print("Please provide a search query.")
