import sys
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC

if len(sys.argv) < 2:
    print("Usage: python_script.py <query>")
    sys.exit(1)

query = "+".join(sys.argv[1:])
query_lower = query.lower()

driver = webdriver.Chrome()

driver.get(f"https://www.mega-image.ro/search?q={query.replace(' ', '+')}%3Arelevance&sort=relevance")

# Handle cookie consent popup if it appears
try:
    cookie_button = WebDriverWait(driver, 10).until(
        EC.element_to_be_clickable((By.ID, "onetrust-accept-btn-handler"))
    )
    cookie_button.click()
    print("Cookie consent popup handled.")
except Exception as e:
    print("No cookie consent popup found or it couldn't be handled:", e)

# Wait for the page to load and display product summaries
try:
    WebDriverWait(driver, 20).until(
        EC.presence_of_element_located((By.CSS_SELECTOR, 'li[class*="product-item"]')))
except Exception as e:
    print("Error waiting for product summary items:", e)

# Find all li elements with class containing 'product-item'
li_elements = driver.find_elements(By.CSS_SELECTOR, 'li[class*="product-item"]')

# Print the text of all <a> elements within each <li> element that match the query exactly and extract prices
for index, li_element in enumerate(li_elements, start=1):
    # Find all <a> elements within the current <li> element
    a_elements = li_element.find_elements(By.TAG_NAME, "a")
    # Extract and print the text of each <a> element
    for a_element in a_elements:
        item_text = a_element.text.strip()
        item_text_lower = item_text.lower()  # Converting item text to lowercase for case-insensitive comparison
        # Check if the query is exactly in the item text
        if query_lower in item_text_lower.split():
            print(f"Text for <a> element in li element {index}: {item_text}")
            # Extract price
            try:
                integer_part = li_element.find_element(By.CSS_SELECTOR, 'span[class*="sc-bw95zp-9"]').text
                fractional_part = li_element.find_element(By.CSS_SELECTOR, 'sup[class*="sc-bw95zp-10"]').text
                price = f"{integer_part}.{fractional_part}"
                print(f"Price: {price}")
            except Exception as e:
                print(f"Price not found for item {index}: {e}")

# Close the browser
driver.quit()
