import unittest
from Auchan import get_product_info  # Import the function from Auchan.py

class TestGetProductInfo(unittest.TestCase):

    # Test for a successful GET request (status code 200)
    def test_get_product_info_success(self):
        query = "product"  # Search for "product"
        exact = False  # Search for any product, not just the exact match
        result = get_product_info(query, exact)  # Call the function

        # Ensure that the result is not None (the function should return some data)
        self.assertIsNotNone(result)
        # Ensure that the result contains at least one product
        self.assertGreater(len(result), 0)
        # Ensure that the product has the expected keys
        self.assertIn('name', result[0])
        self.assertIn('price', result[0])
        self.assertIn('product_url', result[0])

    # Test for the case when no products are found (empty results)
    def test_get_product_info_no_products(self):
        query = "nonexistentproduct"  # Search for a non-existent product
        exact = False
        result = get_product_info(query, exact)  # Call the function

        # Ensure that the result is an empty list, as no products were found
        self.assertEqual(result, [])

    # Test for the case when the GET request fails (invalid URL or error status)
    def test_get_product_info_failed_request(self):
        query = "invalidurl"  # Simulate an invalid URL
        exact = False
        result = get_product_info(query, exact)  # Call the function

        # Ensure that the result is an empty list, as the request failed
        self.assertEqual(result, [])

# Run the tests if this script is executed directly
if __name__ == '__main__':
    unittest.main()
