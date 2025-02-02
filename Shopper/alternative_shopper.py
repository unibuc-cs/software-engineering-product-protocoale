import sys
from utils import carrefour_shopper, megaimage_shopper, auchan_shopper
import threading
from tkinter import filedialog as fd


if __name__ == "__main__":

    filetypes = (("text files", "*.txt"), )

    file = fd.askopenfilename(filetypes=filetypes, initialdir='/home/mario/Downloads/')

    with open(file) as f:

        shopping_list = [i.strip("\n") for i in f.readlines()]

    products = {"Carrefour": [], "Mega Image": [], "Auchan": []}
    for product in shopping_list:

        if "carrefour" in product:
            products["Carrefour"].append(product)

        elif "mega-image" in product:
            products["Mega Image"].append(product)

        elif "auchan" in product:
            products["Auchan"].append(product)

        else:
            print("Can't buy this ", product)

    thread_carrefour = threading.Thread(
        target=carrefour_shopper, args=(products["Carrefour"],)
    )
    thread_megaimage = threading.Thread(
        target=megaimage_shopper, args=(products["Mega Image"],)
    )
    thread_auchan = threading.Thread(target=auchan_shopper, args=(products["Auchan"],))

    if len(products["Carrefour"]):
        thread_carrefour.start()

    if len(products["Auchan"]):
        thread_auchan.start()

    if len(products["Mega Image"]):
        thread_megaimage.start()

    if len(products["Carrefour"]):
        thread_carrefour.join()

    if len(products["Auchan"]):
        thread_auchan.join()

    if len(products["Mega Image"]):
        thread_megaimage.join()
