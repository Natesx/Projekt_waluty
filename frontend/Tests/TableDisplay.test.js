import { render, screen } from '@testing-library/react';
import { TableComponent } from '../src/components/TableComponent';

const sampleData = [
    { effectiveDate: '2024-01-02', currency: 'USD', code: 'USD', mid: 3.94 },
    { effectiveDate: '2024-01-02', currency: 'EUR', code: 'EUR', mid: 4.34 }
];

test('Tabela poprawnie wyświetla dane', () => {
    render(<TableComponent rates={sampleData} />);

    expect(screen.getByText('USD')).toBeInTheDocument();
    expect(screen.getByText('EUR')).toBeInTheDocument();
});
