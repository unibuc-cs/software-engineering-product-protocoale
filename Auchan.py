import sys
import time
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC

# Check if the query is provided as a command-line argument
if len(sys.argv) < 2:
    print("Usage: python_script.py <query>")
    sys.exit(1)

# Extract the query from the command-line argument
query = "+".join(sys.argv[1:])

# Start a new instance of Chrome
driver = webdriver.Chrome()

# Load the webpage with the provided query
driver.get(f"https://www.auchan.ro/search/{query}?_q={query}&map=ft")

# Wait for the page to load and display product summaries
try:
    WebDriverWait(driver, 20).until(
        EC.presence_of_element_located((By.CSS_SELECTOR, 'article[class*="vtex-product-summary"]')))
except Exception as e:
    print("Error waiting for product summary articles:", e)


# Function to scroll down and load more items
def scroll_down_and_load_more(driver, last_height):
    while True:
        # Scroll down to the bottom
        driver.execute_script("window.scrollTo(0, document.body.scrollHeight);")

        # Wait for new elements to load
        time.sleep(5)

        # Check if there's a "Load More" button and click it if present
        try:
            load_more_button = driver.find_element(By.CSS_SELECTOR, 'button[class*="load-more"]')
            if load_more_button:
                load_more_button.click()
                time.sleep(5)  # Wait for more items to load
        except:
            pass

        # Calculate new scroll height and compare with last scroll height
        new_height = driver.execute_script("return document.body.scrollHeight")
        if new_height == last_height:
            break
        last_height = new_height


# Initial scroll height
last_height = driver.execute_script("return document.body.scrollHeight")

# Scroll down to load all items
scroll_down_and_load_more(driver, last_height)

# Find all article elements with class containing 'vtex-product-summary'
articles = driver.find_elements(By.CSS_SELECTOR, 'article[class*="vtex-product-summary"]')

# Print the text inside each article element
for index, article in enumerate(articles, start=1):
    print(article.text.strip())
    print("-" * 50)

# Close the browser
driver.quit()
