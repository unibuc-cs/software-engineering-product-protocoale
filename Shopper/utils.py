import time
from selenium import webdriver
from selenium.webdriver.chrome.service import Service as ChromeService
from webdriver_manager.chrome import ChromeDriverManager
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.common.by import By
import tempfile

#import sys

def get_temp_user_data_dir():
    temp_dir = tempfile.mkdtemp()
    return temp_dir


def carrefour_shopper(args):

    temp_user_data_dir = get_temp_user_data_dir()
    options=Options()
    options.add_argument(f"--user-data-dir={temp_user_data_dir}")  
    options.add_argument("--profile-directory=Default")  
    
    driver1 = webdriver.Chrome(service=ChromeService(ChromeDriverManager().install()),options=options)

    driver1.get("https://carrefour.ro/customer/account/login/referer/aHR0cHM6Ly9jYXJyZWZvdXIucm8vP2xvZ291dFN1Y2Nlc3M9MQ%3D%3D/")

    while driver1.current_url != 'https://carrefour.ro/customer/account/index/':
        pass

    driver1.quit()

    driver2 = webdriver.Chrome(service=ChromeService(ChromeDriverManager().install()),options=options)

    driver2.minimize_window()

    for product in args:
        
        driver2.get(product)
        button =driver2.find_element(By.ID,"product-addtocart-button")

        if button.get_attribute("Title") != "Stoc indisponibil":
            button.click()
            time.sleep(1)

    driver2.get("https://carrefour.ro/new_checkout/cart")
    
    try:

        driver2.set_window_size(1200, 900)

        while len (driver2.window_handles) > 0:
            pass

    except:
        driver2.quit()




def megaimage_shopper(args):

    temp_user_data_dir = get_temp_user_data_dir()
    options=Options()
    options.add_argument(f"--user-data-dir={temp_user_data_dir}")  
    options.add_argument("--profile-directory=Default")  
    
    driver1 = webdriver.Chrome(service=ChromeService(ChromeDriverManager().install()),options=options)

    driver1.get(r"https://www.mega-image.ro/reg/welcome")

    while driver1.current_url!='https://www.mega-image.ro/':
        pass

    driver1.quit()
      
    driver2 = webdriver.Chrome(service=ChromeService(ChromeDriverManager().install()),options=options)

    driver2.minimize_window()

    for product in args:
        
        driver2.get(product)
        time.sleep(1)
        try:
            button =driver2.find_element(By.CSS_SELECTOR,"button[data-testid='product-block-add']")
            button.click()

        except:
            try:
                button =driver2.find_element(By.CSS_SELECTOR,"button[data-testid='product-block-quantity-increase']")
                button.click()
            except:
                pass

    driver2.set_window_size(1200, 900)

    driver2.get(r"https://www.mega-image.ro/checkout")
    
    while len (driver2.window_handles) > 0:
        pass
    
    driver2.quit()




def auchan_shopper(args):

    temp_user_data_dir = get_temp_user_data_dir()
    options=Options()
    options.add_argument(f"--user-data-dir={temp_user_data_dir}")  
    options.add_argument("--profile-directory=Default")  
    
    driver1 = webdriver.Chrome(service=ChromeService(ChromeDriverManager().install()),options=options)
    
    driver1.set_window_size(1200, 900)

    driver1.get(r"https://www.auchan.ro/login")
    
    while driver1.current_url!='https://www.auchan.ro/':
        pass
    
    driver1.quit()
    
    options.add_argument("--headless")
   
    driver2 = webdriver.Chrome(service=ChromeService(ChromeDriverManager().install()),options=options)
    
    for product in args:
        driver2.get(product)
        time.sleep(4)
        try:
            button =driver2.find_element(By.CSS_SELECTOR,"button[class='flex items-center justify-center mb0 pb0 br0 bw0 bg-white auchan-add-to-cart-0-x-plusButton auchan-add-to-cart-0-x-plusButton--productAdd']")
            button.click()
            print(product)

        except:
            button =driver2.find_element(By.CSS_SELECTOR,"button[class='vtex-button bw1 ba fw5 v-mid relative pa0 lh-solid br2 min-h-regular t-action bg-action-primary b--action-primary c-on-action-primary hover-bg-action-primary hover-b--action-primary hover-c-on-action-primary pointer w-100 ']")
            button.click()
            print(product,2)
    
    driver2.quit()
            
    try:
        options=Options()

        options.add_argument(f"--user-data-dir={temp_user_data_dir}")  

        options.add_argument("--profile-directory=Default")  

        driver1 = webdriver.Chrome(service=ChromeService(ChromeDriverManager().install()),options=options)
        
        driver1.set_window_size(1200, 900)

        driver1.get("https://www.auchan.ro/")

        while len (driver1.window_handles) > 0:
                pass
    except:

        driver1.quit()

