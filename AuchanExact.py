import sys
import re
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC

# Check if the query is provided as a command-line argument
if len(sys.argv) < 2:
    print("Usage: python_script.py <query>")
    sys.exit(1)

# Extract the query from the command-line argument
query = " ".join(sys.argv[1:])

# Start a new instance of Chrome
driver = webdriver.Chrome()

# Load the webpage with the provided query
driver.get(f"https://www.auchan.ro/search/{query}?_q={query}&map=ft")

# Wait for a longer time to ensure dynamic content is loaded
try:
    WebDriverWait(driver, 20).until(EC.presence_of_element_located((By.CSS_SELECTOR, 'article[class*="vtex-product-summary"]')))
except Exception as e:
    print("Error waiting for product summary articles:", e)
    driver.quit()
    sys.exit(1)

# Find all article elements with class containing 'vtex-product-summary'
articles = driver.find_elements(By.CSS_SELECTOR, 'article[class*="vtex-product-summary"]')

# Compile a regex pattern to match the query as a whole word
pattern = re.compile(rf'\b{re.escape(query)}\b', re.IGNORECASE)

# Print the text inside each article element that matches the pattern
for index, article in enumerate(articles, start=1):
    article_text = article.text.strip()
    if pattern.search(article_text):
        print(article_text)
        print("-" * 50)

# Close the browser
driver.quit()
