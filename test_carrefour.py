import unittest
from Carrefour import get_product_info


class TestGetProductInfo(unittest.TestCase):

    def test_get_product_info_success(self):
        query = "product"
        exact = False
        result = get_product_info(query, exact)

        self.assertIsNotNone(result)
        self.assertGreater(len(result), 0)
        self.assertIn('name', result[0])
        self.assertIn('price', result[0])
        self.assertIn('product_url', result[0])

    def test_get_product_info_no_products(self):
        query = "nonexistentproduct"
        exact = False
        result = get_product_info(query, exact)

        self.assertEqual(result, [])

    def test_get_product_info_failed_request(self):
        query = "invalidurl"
        exact = False
        result = get_product_info(query, exact)

        self.assertEqual(result, [])


if __name__ == '__main__':
    unittest.main()
