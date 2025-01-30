import { render, screen, fireEvent } from '@testing-library/react';
import App from '../src/App';
import axios from 'axios';

jest.mock('axios');

test('Kliknięcie przycisku pobiera dane', async () => {
    axios.get.mockResolvedValue({ data: [{ effectiveDate: '2024-01-02', currency: 'USD', code: 'USD', mid: 3.94 }] });

    render(<App />);

    const fetchButton = screen.getByText(/Pobierz dane/i);
    fireEvent.click(fetchButton);

    const row = await screen.findByText(/USD/i);
    expect(row).toBeInTheDocument();
});
