import unittest
from Mega import get_product_info

class TestGetProductInfo(unittest.TestCase):

    def test_get_product_info_success(self):
        query = "product"  # Search for a general product
        exact = False  # Search for any product, not just an exact match
        result = get_product_info(query, exact)  # Calling the function

        # Verify that the result is not None
        self.assertIsNotNone(result)
        # Verify that there is at least one product found
        self.assertGreater(len(result), 0)
        # Verify that the result contains 'name'
        self.assertIn('name', result[0])
        # Verify that the result contains 'price'
        self.assertIn('price', result[0])
        # Verify that the result contains 'product_url'
        self.assertIn('product_url', result[0])

    def test_get_product_info_no_products(self):
        # Search for a non-existent product
        result = get_product_info("nonexistentproduct", exact=False)
        # Verify that the result is None for non-existent products
        self.assertIsNone(result)

    def test_get_product_info_failed_request(self):
        query = "invalidurl"  # Simulate an invalid URL
        exact = False
        result = get_product_info(query, exact)  # Calling the function

        # Verify that the result is None for failed requests
        self.assertIsNone(result)

    def test_get_product_info_empty_query(self):
        query = ""  # Search with an empty query
        exact = False
        result = get_product_info(query, exact)
        # Verify that the result is not None for an empty query
        self.assertIsNotNone(result)
        # Verify that at least one product is returned
        self.assertGreater(len(result), 0)

    def test_get_product_info_exact_match(self):
        query = "apa"  # Search for an exact match product
        exact = True
        result = get_product_info(query, exact)
        # Verify that the result is not None
        self.assertIsNotNone(result)
        # Verify that at least one product is returned
        self.assertGreater(len(result), 0)
        # Verify that all returned products have the exact search term in their name
        for product in result:
            self.assertIn(query.lower(), product['name'].lower())

if __name__ == '__main__':
    unittest.main()
