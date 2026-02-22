/**
 * API Client for Event Tickets Manager
 *
 * A typed client for making requests to the backend API.
 */

import type {
  ApiResponse,
  ApiError,
  PagedResponse,
  EventResponse,
  TicketTierResponse,
  CartItemResponse,
  OrderResponse,
  TicketResponse,
  CreateEventRequest,
  CreateTicketTierRequest,
  AddToCartRequest,
  CreateOrderRequest,
} from './api-schema';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

class ApiClient {
  private baseUrl: string;
  private defaultHeaders: HeadersInit;

  constructor(baseUrl: string = API_BASE_URL) {
    this.baseUrl = baseUrl;
    this.defaultHeaders = {
      'Content-Type': 'application/json',
    };
  }

  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const url = `${this.baseUrl}${endpoint}`;
    const config: RequestInit = {
      ...options,
      headers: { ...this.defaultHeaders, ...options.headers },
    };

    const response = await fetch(url, config);

    if (!response.ok) {
      const error: ApiError = await response.json().catch(() => ({
        code: 'UNKNOWN_ERROR',
        message: 'An unknown error occurred',
      }));
      throw new Error(`${error.code}: ${error.message}`);
    }

    return response.json();
  }

  // Events
  async getEvents(params?: { page?: number; pageSize?: number }): Promise<PagedResponse<EventResponse>> {
    const searchParams = new URLSearchParams(params as Record<string, string>);
    return this.request<PagedResponse<EventResponse>>(`/events?${searchParams}`);
  }

  async getEvent(id: string): Promise<EventResponse> {
    return this.request<EventResponse>(`/events/${id}`);
  }

  async createEvent(data: CreateEventRequest): Promise<EventResponse> {
    return this.request<EventResponse>('/events', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  async updateEvent(id: string, data: Partial<CreateEventRequest>): Promise<EventResponse> {
    return this.request<EventResponse>(`/events/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
  }

  async deleteEvent(id: string): Promise<void> {
    return this.request<void>(`/events/${id}`, {
      method: 'DELETE',
    });
  }

  // Ticket Tiers
  async getTicketTiers(eventId: string): Promise<TicketTierResponse[]> {
    return this.request<TicketTierResponse[]>(`/events/${eventId}/tiers`);
  }

  async createTicketTier(eventId: string, data: Omit<CreateTicketTierRequest, 'eventId'>): Promise<TicketTierResponse> {
    return this.request<TicketTierResponse>(`/events/${eventId}/tiers`, {
      method: 'POST',
      body: JSON.stringify({ ...data, eventId }),
    });
  }

  async updateTicketTier(eventId: string, tierId: string, data: Partial<CreateTicketTierRequest>): Promise<TicketTierResponse> {
    return this.request<TicketTierResponse>(`/events/${eventId}/tiers/${tierId}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
  }

  async deleteTicketTier(eventId: string, tierId: string): Promise<void> {
    return this.request<void>(`/events/${eventId}/tiers/${tierId}`, {
      method: 'DELETE',
    });
  }

  // Cart
  async getCart(): Promise<{ items: CartItemResponse[]; totalItems: number; totalAmount: number }> {
    return this.request('/cart');
  }

  async addToCart(data: AddToCartRequest): Promise<ApiResponse<unknown>> {
    return this.request('/cart/items', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  async updateCartItem(tierId: string, quantity: number): Promise<ApiResponse<unknown>> {
    return this.request(`/cart/items/${tierId}`, {
      method: 'PUT',
      body: JSON.stringify({ quantity }),
    });
  }

  async removeFromCart(tierId: string): Promise<ApiResponse<unknown>> {
    return this.request(`/cart/items/${tierId}`, {
      method: 'DELETE',
    });
  }

  async clearCart(): Promise<ApiResponse<unknown>> {
    return this.request('/cart', {
      method: 'DELETE',
    });
  }

  // Orders
  async getOrders(): Promise<OrderResponse[]> {
    return this.request<OrderResponse[]>('/orders');
  }

  async getOrder(id: string): Promise<OrderResponse> {
    return this.request<OrderResponse>(`/orders/${id}`);
  }

  async createOrder(data: CreateOrderRequest): Promise<OrderResponse> {
    return this.request<OrderResponse>('/orders', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  // Health Check
  async healthCheck(): Promise<{ status: string; timestamp: string }> {
    return this.request('/health');
  }
}

export const apiClient = new ApiClient();
