"use client";

import React, { createContext, useContext, useState, useEffect } from "react";
import { apiClient } from "@/api/generated/client";

interface CartItem {
  ticketTierId: string;
  tierName: string;
  price: number;
  quantity: number;
  subtotal: number;
}

interface CartContextType {
  items: CartItem[];
  totalItems: number;
  totalAmount: number;
  loading: boolean;
  addItem: (ticketTierId: string, quantity: number) => Promise<void>;
  updateItem: (ticketTierId: string, quantity: number) => Promise<void>;
  removeItem: (ticketTierId: string) => Promise<void>;
  clearCart: () => Promise<void>;
  refreshCart: () => Promise<void>;
}

const CartContext = createContext<CartContextType | undefined>(undefined);

export function CartProvider({ children }: { children: React.ReactNode }) {
  const [items, setItems] = useState<CartItem[]>([]);
  const [loading, setLoading] = useState(false);

  const totalItems = items.reduce((sum, item) => sum + item.quantity, 0);
  const totalAmount = items.reduce((sum, item) => sum + item.subtotal, 0);

  const refreshCart = async () => {
    try {
      setLoading(true);
      const response = await apiClient.getCart();
      setItems(response.items || []);
    } catch (error) {
      console.error("Failed to load cart:", error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    refreshCart();
  }, []);

  const addItem = async (ticketTierId: string, quantity: number) => {
    try {
      setLoading(true);
      const response = await fetch(`/api/cart/items`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ ticketTierId, quantity }),
        credentials: "include",
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || "Failed to add item to cart");
      }

      await refreshCart();
    } catch (error) {
      throw error;
    } finally {
      setLoading(false);
    }
  };

  const updateItem = async (ticketTierId: string, quantity: number) => {
    try {
      setLoading(true);
      await fetch(`/api/cart/items/${ticketTierId}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ quantity }),
        credentials: "include",
      });

      await refreshCart();
    } catch (error) {
      console.error("Failed to update cart item:", error);
    } finally {
      setLoading(false);
    }
  };

  const removeItem = async (ticketTierId: string) => {
    try {
      setLoading(true);
      await fetch(`/api/cart/items/${ticketTierId}`, {
        method: "DELETE",
        credentials: "include",
      });

      await refreshCart();
    } catch (error) {
      console.error("Failed to remove cart item:", error);
    } finally {
      setLoading(false);
    }
  };

  const clearCart = async () => {
    try {
      setLoading(true);
      await fetch(`/api/cart`, {
        method: "DELETE",
        credentials: "include",
      });

      setItems([]);
    } catch (error) {
      console.error("Failed to clear cart:", error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <CartContext.Provider
      value={{
        items,
        totalItems,
        totalAmount,
        loading,
        addItem,
        updateItem,
        removeItem,
        clearCart,
        refreshCart,
      }}
    >
      {children}
    </CartContext.Provider>
  );
}

export function useCart() {
  const context = useContext(CartContext);
  if (!context) {
    throw new Error("useCart must be used within a CartProvider");
  }
  return context;
}
