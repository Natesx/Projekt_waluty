import React, { useState, useEffect, useMemo } from 'react';
import axios from 'axios';
import { format, getYear, getQuarter, getMonth } from 'date-fns';
import { useTable } from 'react-table';
import Select from 'react-select';

const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:8080';

function App() {
    const [startDate, setStartDate] = useState('');
    const [endDate, setEndDate] = useState('');
    const [rates, setRates] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);
    const [filterType, setFilterType] = useState('all');
    const [selectedYear, setSelectedYear] = useState(null);
    const [selectedQuarter, setSelectedQuarter] = useState(null);
    const [selectedMonth, setSelectedMonth] = useState(null);
    const [selectedDay, setSelectedDay] = useState(null);

    // Pobieranie kursów walut
    const fetchRates = async () => {
        setLoading(true);
        try {
            let response;
            if (filterType === 'day' && selectedDay) {
                response = await axios.get(`${API_URL}/currencies/${selectedDay.value}`);
            } else {
                response = await axios.get(`${API_URL}/currencies/rates`, {
                    params: {
                        startDate,
                        endDate,
                        filterType,
                        year: selectedYear?.value || null,
                        quarter: selectedQuarter?.value || null,
                        month: selectedMonth?.value || null
                    }
                });
            }
            setRates(response.data);
        } catch (err) {
            setError('Błąd pobierania kursów walut');
            console.error("Rates fetch error:", err);
        } finally {
            setLoading(false);
        }
    };

    const fetchAndUpdateRates = async () => {
      if (!startDate || !endDate) {
          alert("⚠️ Wybierz zakres dat!");
          return;
      }
  
      setLoading(true);
      setError(null);
  
      try {
          // 📡 Najpierw pobierz dane z API NBP i zapisz do bazy
          await axios.post(`${API_URL}/currencies/fetch/${format(new Date(startDate), 'yyyy-MM-dd')}/${format(new Date(endDate), 'yyyy-MM-dd')}`);
          console.log("✅ Dane pobrane z API NBP i zapisane do bazy.");
  
          // ⏳ Poczekaj 3 sekundy na zapis danych do bazy, zanim pobierzesz je ponownie
          setTimeout(async () => {
              await fetchRates();
          }, 3000);
      } catch (err) {
          setError("❌ Błąd pobierania danych z NBP");
          console.error("Fetch error:", err);
      } finally {
          setLoading(false);
      }
  };
  
  

    // Pobieranie walut do filtrów
    const fetchCurrencies = async () => {
        try {
            const response = await axios.get(`${API_URL}/currencies`);
            console.log("Dostępne waluty:", response.data);
        } catch (err) {
            console.error("Błąd pobierania walut:", err);
        }
    };

    // Pobieranie danych przy zmianie filtrów
    useEffect(() => {
        fetchRates();
    }, [startDate, endDate, filterType, selectedYear, selectedQuarter, selectedMonth, selectedDay]);

    useEffect(() => {
        fetchCurrencies();
    }, []);

    // Opcje filtrów
    const filterOptions = [
        { value: 'all', label: 'Wszystkie' },
        { value: 'year', label: 'Rok' },
        { value: 'quarter', label: 'Kwartał' },
        { value: 'month', label: 'Miesiąc' },
        { value: 'day', label: 'Dzień' }
    ];

    const filteredRates = useMemo(() => rates, [rates]);

    const columns = useMemo(() => [
        { Header: 'Data', accessor: 'effectiveDate', Cell: ({ value }) => format(new Date(value), 'yyyy-MM-dd') },
        { Header: 'Waluta', accessor: 'currency' },
        { Header: 'Kod', accessor: 'code' },
        { Header: 'Kurs', accessor: 'mid' }
    ], []);

    const tableInstance = useTable({ columns, data: filteredRates });

    return (
        <div style={{ padding: '20px', maxWidth: '1200px', margin: '0 auto' }}>
            <h2>Wybierz zakres dat</h2>
            <input type="date" value={startDate} onChange={e => setStartDate(e.target.value)} />
            <input type="date" value={endDate} onChange={e => setEndDate(e.target.value)} />
            <button onClick={fetchAndUpdateRates} disabled={loading}>
    {loading ? 'Pobieranie...' : 'Pobierz dane'}
</button>

            {error && <p style={{ color: 'red' }}>{error}</p>}

            <h2>Kursy walut</h2>
            <Select options={filterOptions} onChange={e => setFilterType(e.value)} defaultValue={filterOptions[0]} />

            {filterType === 'year' && <Select options={[...new Set(rates.map(rate => ({ value: getYear(new Date(rate.effectiveDate)), label: getYear(new Date(rate.effectiveDate)) })))]} onChange={setSelectedYear} />}
            {filterType === 'quarter' && <Select options={[{ value: 1, label: 'Q1' }, { value: 2, label: 'Q2' }, { value: 3, label: 'Q3' }, { value: 4, label: 'Q4' }]} onChange={setSelectedQuarter} />}
            {filterType === 'month' && <Select options={Array.from({ length: 12 }, (_, i) => ({ value: i + 1, label: format(new Date(2024, i, 1), 'MMMM') }))} onChange={setSelectedMonth} />}
            {filterType === 'day' && <Select options={rates.map(rate => ({ value: rate.effectiveDate, label: format(new Date(rate.effectiveDate), 'yyyy-MM-dd') }))} onChange={setSelectedDay} />}

            <table border="1" style={{ width: '100%' }}>
                <thead>
                    {tableInstance.headerGroups.map(headerGroup => (
                        <tr {...headerGroup.getHeaderGroupProps()}>
                            {headerGroup.headers.map(column => (
                                <th {...column.getHeaderProps()}>{column.render('Header')}</th>
                            ))}
                        </tr>
                    ))}
                </thead>
                <tbody>
                    {tableInstance.rows.map(row => {
                        tableInstance.prepareRow(row);
                        return (
                            <tr {...row.getRowProps()}>
                                {row.cells.map(cell => (
                                    <td {...cell.getCellProps()}>{cell.render('Cell')}</td>
                                ))}
                            </tr>
                        );
                    })}
                </tbody>
            </table>
        </div>
    );
}

export default App;
